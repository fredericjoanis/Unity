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

    [System.Serializable]
    public struct MonoBehaviorData : IMonoBehaviorData
    {
        public int Bla;

        public void Execute()
        {
            Bla++;
        }

        public void ProcessMessage(IMessage message)
        {
            switch(message)
            {
                case FooMessage foo:
                    Bla += foo.SomeFooValue;
                    break;
                case BarMessage bar:
                    Bla += bar.SomeBarValue;
                    break;
                default:
                    Debug.Assert(false, "Wrong Mailbox or unsupported message!");
                    break;
            }
        }
    }

    //[BurstCompile]
    //public class MonoBehaviorTest : MonoBehaviorMainThread<MonoBehaviorJob>
    public class MonoBehaviorTest : MonoBehaviorJob<MonoBehaviorData>
    {
        public MonoBehaviorData initialMonoBehaviorData;
        protected override MonoBehaviorData InitialData { get { return initialMonoBehaviorData; } }
    }
}