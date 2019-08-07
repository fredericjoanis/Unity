using System.Runtime.InteropServices;
using UnityEngine;


namespace Prototype
{
    [System.Serializable]
    public struct ChestData
    {
        public int moneyAmount;
    }

    public class ChestManager : Manager<ChestData, Chest>
    {
        public void PickupChest(ref MonoBehaviourJobData monoBehaviorJobData)
        {
            MoneyTransaction moneyTransaction = new MoneyTransaction() { Amount = monoBehaviorJobData.UserData.moneyAmount };
            SendMessage(ref monoBehaviorJobData, PlayerManager.Instance.Data[0].guid, new Message() { messageEnum = MessageEnum.MoneyTransaction, moneyTransaction = moneyTransaction });
            monoBehaviorJobData.UserData.moneyAmount = 0;
        }

        public override void Update(ref MonoBehaviourJobData monoBehaviorJobData)
        {
            monoBehaviorJobData.UserData.moneyAmount++;
        }

        public override void ProcessMessage(ref MonoBehaviourJobData monoBehaviorJobData, Message message)
        {
            switch (message.messageEnum)
            {
                case MessageEnum.PickupChest:
                    PickupChest(ref monoBehaviorJobData);
                    break;
                default:
                    Debug.Assert(false, "Wrong Mailbox or unsupported message!");
                    break;
            }
        }
    }

    public class Chest : MonoBehaviourJob<ChestData>
    {
    }
}
