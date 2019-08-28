using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Prototype
{
    public struct ChestPickup : IComponentData
    {
        public Entity WhoIsPicking;
    }

    [System.Serializable]
    public struct ChestData : IComponentData
    {
        public int moneyAmount;
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

    public class ChestPickupSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, ref ChestData data, ref ChestPickup pickup) =>
            {
                EntityManager.AddComponentData(pickup.WhoIsPicking, new MoneyTransaction() { Amount = data.moneyAmount });
                data.moneyAmount = 0;
                EntityManager.RemoveComponent(entity, typeof(ChestPickup));
            });
        }
    }

    public class Chest : ECS<ChestData>
    {
    }
}