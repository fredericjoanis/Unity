using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System.Runtime.InteropServices;
using Unity.Burst;

namespace Prototype
{
    //[BurstCompile]
    public struct JobArguments<Data> where Data : struct
    {
        [ReadOnly] public Guid Id;
        [WriteOnly] public NativeQueue<Message> MessagesToSend;
        public Data data;

        public void SendMessage(Guid componentGuid, IMessage message)
        {
            MessagesToSend.Enqueue(new Message()
            {
                componentGuid = componentGuid,
                message = Marshal.GetIUnknownForObject(message)
            });
        }
    }

    public interface IJobExecute<Data> where Data : struct
    {
        void Execute(ref JobArguments<Data> args);

        void ProcessMessage(ref JobArguments<Data> args, IMessage message);
    }

    public struct JobExecute<Data, Processing> : IJob
        where Data : struct
        where Processing : struct, IJobExecute<Data>
    {
        public NativeArray<Data> data;
        public JobArguments<Data> args;
        public Processing functionalProcessing;
        [ReadOnly] public NativeArray<IntPtr> MailBox;

        public void Execute()
        {
            args.data = data[0];

            for(int i = 0; i < MailBox.Length; i++)
            { 
                IMessage message = (IMessage)Marshal.GetObjectForIUnknown(MailBox[i]);
                functionalProcessing.ProcessMessage(ref args, message);
            }

            functionalProcessing.Execute(ref args);

            data[0] = args.data;
        }
    }

    public class MailBoxManager
    {
        public static MailBoxManager Instance = new MailBoxManager();
        public Dictionary<Guid, List<IntPtr>> MailBoxes;

        public MailBoxManager()
        {
            MailBoxes = new Dictionary<Guid, List<IntPtr>>();
        }
    }

    public abstract class MonoBehaviorCommon<DataType, Processing> : MonoBehaviour
        where DataType : struct
        where Processing : struct, IJobExecute<DataType>
    {
        protected JobExecute<DataType, Processing> jobExecute;

        protected NativeArray<DataType> Data;
        public DataType InitialData;
        public List<IntPtr> MailBoxAsync;

        public virtual void Awake()
        {
            Data = new NativeArray<DataType>(1, Allocator.Persistent);
            Data[0] = InitialData;
            jobExecute.data = Data;
            jobExecute.args = new JobArguments<DataType>() { Id = Guid.NewGuid() };
            jobExecute.args.MessagesToSend = new NativeQueue<Message>(Allocator.Persistent);
            MailBoxAsync = new List<IntPtr>();

            MailBoxManager.Instance.MailBoxes.Add(jobExecute.args.Id, new List<IntPtr>());

            Manager.Instance.Components.Add(jobExecute);
        }

        public virtual void Update()
        {
            MailBoxManager.Instance.MailBoxes.TryGetValue(jobExecute.args.Id, out List<IntPtr> messages);

            MailBoxAsync.Clear();
            MailBoxAsync.Capacity = messages.Count;

            for(int i = 0; i < messages.Count; i++)
            {
                IMessage message = (IMessage)Marshal.GetObjectForIUnknown(messages[i]);
                if (ProcessMessage(ref jobExecute.args, message) == MessageState.SendToJob)
                {
                    MailBoxAsync.Add(messages[i]);
                }
            }

            if(jobExecute.MailBox.IsCreated)
            {
                jobExecute.MailBox.Dispose();
            }

            jobExecute.MailBox = new NativeArray<IntPtr>(MailBoxAsync.Count, Allocator.TempJob);
            jobExecute.MailBox.CopyFrom(MailBoxAsync.ToArray());
            messages.Clear();
        }

        protected void ProcessMailbox()
        {
            while (jobExecute.args.MessagesToSend.Count != 0)
            {
                Message message = jobExecute.args.MessagesToSend.Dequeue();
                MailBoxManager.Instance.MailBoxes[message.componentGuid].Add(message.message);
            }
        }

        // If you need to work with the GameObject or call anything from the MainThread override this function.
        public virtual MessageState ProcessMessage(ref JobArguments<DataType> args, IMessage message)
        {
            return MessageState.SendToJob;
        }

        public virtual void OnDestroy()
        {
            int? componentIndex = Manager.Instance.GetComponentIndex(jobExecute);
            if(componentIndex.HasValue)
            {
                Manager.Instance.Components.RemoveAtSwapBack(componentIndex.Value);
            }

            MailBoxManager.Instance.MailBoxes.Remove(jobExecute.args.Id);

            jobExecute.data.Dispose();
            jobExecute.MailBox.Dispose();
            jobExecute.args.MessagesToSend.Dispose();
            Data.Dispose();
        }

        public class Manager
        {
            public static Manager Instance = new Manager();
            public List<JobExecute<DataType, Processing>> Components { get; set; }

            public Manager()
            {
                Components = new List<JobExecute<DataType, Processing>>();
            }

            // Warning: O(n) function.
            public int? GetComponentIndex(JobExecute<DataType, Processing> toFind)
            {
                for (int i = 0; i < Components.Count; i++)
                {
                    if (toFind.data == Components[i].data)
                    {
                        return i;
                    }
                }

                return null;
            }
        }
    }

    public abstract class MonoBehaviorJob<Data, Processing> : MonoBehaviorCommon<Data, Processing>
        where Data : struct
        where Processing : struct, IJobExecute<Data>
    {
        JobHandle jobHandle;

        public override void Update()
        {
            base.Update();
            jobHandle = jobExecute.Schedule();
        }

        public virtual void LateUpdate()
        {
            jobHandle.Complete();
            ProcessMailbox();
        }
    }

    public abstract class MonoBehaviorMainThread<Data, Processing> : MonoBehaviorCommon<Data, Processing>
        where Data : struct
        where Processing : struct, IJobExecute<Data>
    {
        public override void Update()
        {
            jobExecute.Execute();
            ProcessMailbox();
        }
    }
}