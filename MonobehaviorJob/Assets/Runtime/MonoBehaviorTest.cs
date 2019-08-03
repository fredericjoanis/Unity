using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Prototype
{
    public struct FooMessage : IMessage
    {
        public int SomeFooValue;
    }

    public struct BarMessage : IMessage
    {
        public int SomeBarValue;
    }

    public struct MonoBehaviourTestJob : IJobExecute<MonoBehaviorTestData>
    {
        public void Execute(ref JobProcessingArgs<MonoBehaviorTestData> args)
        {
            args.data.Bla++;
        }

        public void ProcessMessage(ref JobProcessingArgs<MonoBehaviorTestData> args, IMessage message)
        {
            switch (message)
            {
                case FooMessage foo:
                    args.data.Bla += foo.SomeFooValue;
                    break;
                case BarMessage bar:
                    args.data.Bla += bar.SomeBarValue;
                    break;
                default:
                    Debug.Assert(false, "Wrong Mailbox or unsupported message!");
                    break;
            }
        }
    }

    [System.Serializable]
    public struct MonoBehaviorTestData
    {
        public int Bla;
    }

    //public class MonoBehaviorTest : MonoBehaviorMainThread<MonoBehaviorJob>
    public class MonoBehaviorTest : MonoBehaviorJob<MonoBehaviorTestData, MonoBehaviourTestJob>
    {
        public MonoBehaviorTestData initialMonoBehaviorData;
        protected override MonoBehaviorTestData InitialData { get { return initialMonoBehaviorData; } }
    }
}