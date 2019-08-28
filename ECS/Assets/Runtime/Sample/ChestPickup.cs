using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Prototype
{
    public struct ChestPickup : IComponentData
    {
        public Entity WhoIsPicking;
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
}