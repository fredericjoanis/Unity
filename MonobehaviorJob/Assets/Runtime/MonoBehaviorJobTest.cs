using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Prototype
{
    [System.Serializable]
    public struct MonoBehaviorJob : IMonoBehaviorJob
    {
        public int Bla;
        public bool Blip;

        public void Execute()
        {
            Bla++;
            Blip = !Blip;
        }
    }

    //public class MonoBehaviorTest : MonoBehaviorMainThread<MonoBehaviorJob>
    public class MonoBehaviorTest : MonoBehaviorJob<MonoBehaviorJob>
    {
        public MonoBehaviorJob monoBehaviorJob;

        public override MonoBehaviorJob InitialData { get { return monoBehaviorJob; } }
    }
}