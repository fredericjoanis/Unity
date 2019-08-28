using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Prototype
{
    public struct MoneyTransaction : IComponentData
    {
        public int Amount;
    }

    [System.Serializable]
    public struct PlayerData : IComponentData
    {
        public int TotalMoney;
        public Entity ChestToOpen;
    }

    // HybridECS
    public class PlayerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, ref PlayerData data, Player player) =>
            {
                if (player.random.Next(0, 10) >= 9)
                {
                    // Workaround until I understand the Conversion pipeline better.
                    data.ChestToOpen = player.ChestToOpen.Entity;
                    EntityManager.AddComponentData(data.ChestToOpen, new ChestPickup() { WhoIsPicking = entity });
                }
            });
        }
    }

    public class PlayerMoneyTransactionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, ref PlayerData data, ref MoneyTransaction moneyTransaction) =>
            {
                data.TotalMoney += moneyTransaction.Amount;
                EntityManager.RemoveComponent(entity, typeof(MoneyTransaction));
            });
        }
    }

    public class Player : ECS<PlayerData>
    {
        // Using a non-blittable type. You should use Unity.Mathematics.Random, but this is an example.
        public System.Random random = new System.Random();
        public Chest ChestToOpen;
        
        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            // Todo : Chest class Convert function is called after this function. GetPrimaryEntity is not working in this case.
            EntityData.ChestToOpen = conversionSystem.GetPrimaryEntity(ChestToOpen);
            base.Convert(entity, dstManager, conversionSystem);
        }
    }
}