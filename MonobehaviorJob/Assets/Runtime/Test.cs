using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prototype
{
    [System.Serializable]
    public struct TestData
    {
        public int Bla;
    }

    public class TestManager : Manager<TestData, Test>
    {
        public override void ProcessMessage(ref MonoBehaviourJobData monoBehaviorJobData, Message message)
        {
        }

        public override void Update(ref MonoBehaviourJobData monoBehaviorJobData)
        {
            monoBehaviorJobData.UserData.Bla++;
        }
    }

    public class Test : MonoBehaviourJob<TestData>
    {
    }
}