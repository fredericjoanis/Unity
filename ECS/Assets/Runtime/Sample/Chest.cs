using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Prototype
{
    [System.Serializable]
    public struct ChestData : IComponentData
    {
        public int moneyAmount;
    }

    public class Chest : ECS<ChestData>
    {
    }

    public class ChestSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref ChestData data) =>
            {
                data.moneyAmount++;
            });
        }
    }
}