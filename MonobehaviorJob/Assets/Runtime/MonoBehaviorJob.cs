using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System.Linq;
using Unity.Burst;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Linq.Expressions;
using System.Reflection;

namespace Prototype
{
    public delegate void FunctionVoid<Data>(ref Data data);


    public interface IMessage
    {
    }

    public interface IMonoBehaviorData
    {
        void Execute();
        void ProcessMessage(IMessage message);
    }

    //[BurstCompile]
    public struct JobKeepValues<Data> : IJob
        where Data : struct, IMonoBehaviorData
    {
        public NativeArray<Data> data;
        public NativeQueue<IntPtr> MailBox;
        public Guid Id;

        public void AddMessage(IMessage message)
        {
            MailBox.Enqueue(Marshal.GetIUnknownForObject(message));
        }

        public void Execute()
        {
            Data dataToExecute = data[0];

            while (MailBox.Count > 0)
            {
                if (MailBox.TryDequeue(out IntPtr ptr))
                {
                    IMessage message = (IMessage)Marshal.GetObjectForIUnknown(ptr);
                    dataToExecute.ProcessMessage(message);
                }
            }

            dataToExecute.Execute();

            data[0] = dataToExecute;
        }
    }

    public abstract class MonoBehaviorCommon<Data> : MonoBehaviour
        where Data : struct, IMonoBehaviorData
    {
        protected JobKeepValues<Data> jobKeepValues;

        protected abstract Data InitialData { get; }

        public virtual void Awake()
        {
            jobKeepValues.MailBox = new NativeQueue<IntPtr>(Allocator.Persistent);
            jobKeepValues.data = new NativeArray<Data>(1, Allocator.Persistent);
            jobKeepValues.data[0] = InitialData;
            jobKeepValues.Id = new Guid();

            Manager.Instance.Components.Add(jobKeepValues);
        }

        public virtual void OnDestroy()
        {
            int? componentIndex = Manager.Instance.GetComponentIndex(jobKeepValues);
            if(componentIndex.HasValue)
            {
                Manager.Instance.Components.RemoveAtSwapBack(componentIndex.Value);
            }
            
            jobKeepValues.data.Dispose();
            jobKeepValues.MailBox.Dispose();
        }

        public class Manager : IDisposable
        {
            public static Manager Instance = new Manager();
            public List<JobKeepValues<Data>> Components { get; set; }

            public Manager()
            {
                Components = new List<JobKeepValues<Data>>();
            }

            // Warning: O(n) function.
            public int? GetComponentIndex(JobKeepValues<Data> toFind)
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

            public void Dispose()
            {
                //Instance.Components.Dispose();
            }
        }
    }

    public abstract class MonoBehaviorJob<Data> : MonoBehaviorCommon<Data>
        where Data : struct, IMonoBehaviorData
    {
        JobHandle jobHandle;

        public virtual void Update()
        {
            jobHandle = jobKeepValues.Schedule();
        }

        public virtual void LateUpdate()
        {
            jobHandle.Complete();
        }
    }

    public abstract class MonoBehaviorMainThread<Data> : MonoBehaviorCommon<Data>
        where Data : struct, IMonoBehaviorData
    {
        public virtual void Update()
        {
            jobKeepValues.Execute();
        }
    }
}