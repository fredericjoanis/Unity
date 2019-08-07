using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;


namespace Prototype
{
    public struct MoneyTransaction
    {
        public int Amount;
    }

    public partial struct Message
    {
        [FieldOffset(2)]
        public MoneyTransaction moneyTransaction;
    }

    [System.Serializable]
    public struct PlayerData
    {
        public int totalMoney;
        public Unity.Mathematics.Random random;
    }

    public class PlayerManager : Manager<PlayerData, Player>
    {

        public override void ProcessMessage(ref MonoBehaviourJobData monoBehaviorJobData, Message message)
        {
            switch (message.messageEnum)
            {
                case MessageEnum.PickupChest:
                    monoBehaviorJobData.UserData.totalMoney += message.moneyTransaction.Amount;
                    break;
                default:
                    Debug.Assert(false, "Wrong Mailbox or unsupported message!");
                    break;
            }
        }

        public override void Update(ref MonoBehaviourJobData monoBehaviorJobData)
        {
            if (monoBehaviorJobData.UserData.random.NextInt(0, 10) > 5)
            {
                SendMessage(ref monoBehaviorJobData, ChestManager.Instance.Data[0].guid, new Message() { messageEnum = MessageEnum.PickupChest });
            }
        }
    }

    public class Player : MonoBehaviourJob<PlayerData>
    {
        public override void Awake()
        {
            base.Awake();
            PlayerData playerData = InitialData;
            playerData.random.InitState(38);
            InitialData = playerData;
        }
    }
}
