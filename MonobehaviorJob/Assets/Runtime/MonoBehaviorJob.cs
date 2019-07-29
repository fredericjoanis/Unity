using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Prototype
{
    public interface IMonoBehaviorJob
    {
        void Execute();
    }

    public struct JobKeepValues<T> : IJob where T : struct, IMonoBehaviorJob
    {
        public NativeArray<T> data;

        public void Execute()
        {
            T dataToExecute = data[0];
            dataToExecute.Execute();
            data[0] = dataToExecute;
        }
    }

    public abstract class MonoBehaviorCommon<T> : MonoBehaviour where T : struct, IMonoBehaviorJob
    {
        public JobKeepValues<T> job;

        public abstract T InitialData { get; }

        void Start()
        {
            job.data = new NativeArray<T>(1, Allocator.Persistent);
            job.data[0] = InitialData;
        }

        private void OnDestroy()
        {
            job.data.Dispose();
        }
    }

    public abstract class MonoBehaviorJob<T> : MonoBehaviorCommon<T> where T : struct, IMonoBehaviorJob
    {
        JobHandle jobHandle;

        void Update()
        {
            jobHandle = job.Schedule();
        }

        private void LateUpdate()
        {
            jobHandle.Complete();
        }
    }

    public abstract class MonoBehaviorMainThread<T> : MonoBehaviorCommon<T> where T : struct, IMonoBehaviorJob
    {
        void Update()
        {
            job.Execute();
        }
    }
}