using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;


namespace Prototype
{
    //[BurstCompile]
    public struct PickupChestMessage : IMessage
    {
        public void PickupChest(ref ChestData data)
        {
            Player.Manager.Instance.Components[0].AddMessage(new MoneyTransaction() { Amount = data.moneyAmount });
            data.moneyAmount = 0;
        }
    }

    [System.Serializable]
    //[BurstCompile]
    public struct ChestData : IMonoBehaviorData
    {
        public int moneyAmount;
        public void Execute()
        {
            moneyAmount++;
        }

        public void ProcessMessage(IMessage message)
        {
            switch (message)
            {
                case PickupChestMessage chest:
                    chest.PickupChest(ref this);
                    break;
                default:
                    Debug.Assert(false, "Wrong Mailbox or unsupported message!");
                    break;
            }
        }
        
    }

    public class Chest : MonoBehaviorJob<ChestData>
    {
        public ChestData initialInteractableData;

        protected override ChestData InitialData { get { return initialInteractableData; } }
    }
}
