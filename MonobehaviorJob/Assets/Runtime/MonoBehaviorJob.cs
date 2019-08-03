using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System.Runtime.InteropServices;
using Unity.Burst;

namespace Prototype
{
    public interface IMessage
    {
    }

    public struct Message
    {
        public Guid componentGuid;
        public IntPtr message;
    }

    //[BurstCompile]
    public struct JobProcessingArgs<Data> where Data : struct
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
        void Execute(ref JobProcessingArgs<Data> args);
        void ProcessMessage(ref JobProcessingArgs<Data> args, IMessage message);
    }

    public struct JobExecute<Data, Processing> : IJob
        where Data : struct
        where Processing : struct, IJobExecute<Data>
    {
        public NativeArray<Data> data;
        public JobProcessingArgs<Data> args;
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

    public abstract class MonoBehaviorCommon<Data, Processing> : MonoBehaviour
        where Data : struct
        where Processing : struct, IJobExecute<Data>
    {
        protected JobExecute<Data, Processing> jobKeepValues;

        protected abstract Data InitialData { get; }

        public virtual void Awake()
        {
            jobKeepValues.data = new NativeArray<Data>(1, Allocator.Persistent);
            jobKeepValues.data[0] = InitialData;
            jobKeepValues.args = new JobProcessingArgs<Data>() { Id = Guid.NewGuid() };
            
            MailBoxManager.Instance.MailBoxes.Add(jobKeepValues.args.Id, new List<IntPtr>());

            Manager.Instance.Components.Add(jobKeepValues);
        }

        public virtual void Update()
        {
            List<IntPtr> messages;
            MailBoxManager.Instance.MailBoxes.TryGetValue(jobKeepValues.args.Id, out messages);

            if(jobKeepValues.MailBox.IsCreated)
            {
                jobKeepValues.MailBox.Dispose();
            }

            if (jobKeepValues.args.MessagesToSend.IsCreated)
            {
                jobKeepValues.args.MessagesToSend.Dispose();
            }

            jobKeepValues.args.MessagesToSend = new NativeQueue<Message>(Allocator.TempJob);

            jobKeepValues.MailBox = new NativeArray<IntPtr>(messages.Count, Allocator.TempJob);
            jobKeepValues.MailBox.CopyFrom(messages.ToArray());
            messages.Clear();
        }

        protected void ProcessMailbox()
        {
            while (jobKeepValues.args.MessagesToSend.Count != 0)
            {
                Message message = jobKeepValues.args.MessagesToSend.Dequeue();
                MailBoxManager.Instance.MailBoxes[message.componentGuid].Add(message.message);
            }
        }

        public virtual void OnDestroy()
        {
            int? componentIndex = Manager.Instance.GetComponentIndex(jobKeepValues);
            if(componentIndex.HasValue)
            {
                Manager.Instance.Components.RemoveAtSwapBack(componentIndex.Value);
            }

            MailBoxManager.Instance.MailBoxes.Remove(jobKeepValues.args.Id);

            jobKeepValues.data.Dispose();
            jobKeepValues.MailBox.Dispose();
        }

        public class Manager
        {
            public static Manager Instance = new Manager();
            public List<JobExecute<Data, Processing>> Components { get; set; }

            public Manager()
            {
                Components = new List<JobExecute<Data, Processing>>();
            }

            // Warning: O(n) function.
            public int? GetComponentIndex(JobExecute<Data, Processing> toFind)
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
            jobHandle = jobKeepValues.Schedule();
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
            jobKeepValues.Execute();
            ProcessMailbox();
        }
    }
}