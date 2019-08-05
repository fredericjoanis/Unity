using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Prototype
{
    public struct IJobExecute2 : IJob
    {
        [ReadOnly] public NativeQueue<IntPtr> ToUpdate;
        public void Execute()
        {
            while(ToUpdate.Count > 0)
            {
                ((IJobExecute)Marshal.GetObjectForIUnknown(ToUpdate.Dequeue())).Execute();
            }
        }
    }

    public class JobManager : MonoBehaviour
    {
        public static JobManager Instance = new JobManager();
        private static JobHandle[] jobHandles = new JobHandle[System.Environment.ProcessorCount];
        private static IJobExecute2[] jobExecuter = new IJobExecute2[System.Environment.ProcessorCount];
        private static int currentJob = 0;

        public void AddJob(ref JobExecute<ChestData, ChestJob> toAdd)
        {
            jobExecuter[currentJob].ToUpdate.Enqueue(Marshal.GetIUnknownForObject(toAdd));
            currentJob++;
            if(currentJob >= System.Environment.ProcessorCount)
            {
                currentJob = 0;
            }
        }

        public virtual void Update()
        {
            for(int i = 0; i < jobExecuter.Length; i++)
            {
                jobHandles[i] = jobExecuter[i].Schedule();
            }
        }

        public virtual void LateUpdate()
        {
            for (int i = 0; i < jobExecuter.Length; i++)
            {
                jobHandles[i].Complete();
                jobExecuter[i].ToUpdate.Dispose();
            }
        }
    }
}