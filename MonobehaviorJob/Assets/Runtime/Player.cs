using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;


namespace Prototype
{
    public struct MoneyTransaction : IMessage
    {
        public int Amount;
    }

    [System.Serializable]
    public struct PlayerData : IMonoBehaviorData
    {
        public int totalMoney;
        public Unity.Mathematics.Random random;

        public void Execute()
        {
            if(random.NextInt(0,10) > 5)
            {
                Chest.Manager.Instance.Components[0].AddMessage(new PickupChestMessage());
            }
        }

        public void ProcessMessage(IMessage message)
        {
            switch (message)
            {
                case MoneyTransaction money:
                    totalMoney += money.Amount;
                    break;
                default:
                    Debug.Assert(false, "Wrong Mailbox or unsupported message!");
                    break;
            }
        }
    }

    public class Player : MonoBehaviorJob<PlayerData>
    {
        public PlayerData initialPlayerData;

        protected override PlayerData InitialData => initialPlayerData;

        public override void Awake()
        {
            initialPlayerData.random.InitState(38);
            base.Awake();
        }
    }
}
