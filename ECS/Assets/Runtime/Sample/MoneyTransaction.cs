using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

namespace Prototype
{
    public struct MoneyTransaction : IComponentData
    {
        public int Amount;
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
}