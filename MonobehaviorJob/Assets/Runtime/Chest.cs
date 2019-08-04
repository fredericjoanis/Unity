using UnityEngine;


namespace Prototype
{
    public struct PickupChestMessage : IMessage
    {
        public void PickupChest(ref JobArguments<ChestData> args)
        {
            args.SendMessage(Player.Manager.Instance.Components[0].args.Id, new MoneyTransaction() { Amount = args.data.moneyAmount });
            args.data.moneyAmount = 0;
        }
    }

    [System.Serializable]
    public struct ChestData
    {
        public int moneyAmount;
    }

    public struct ChestJob : IJobExecute<ChestData>
    {
        public void Execute(ref JobArguments<ChestData> args)
        {
            args.data.moneyAmount++;
        }

        public void ProcessMessage(ref JobArguments<ChestData> args, IMessage message)
        {
            switch (message)
            {
                case PickupChestMessage chest:
                    chest.PickupChest(ref args);
                    break;
                default:
                    Debug.Assert(false, "Wrong Mailbox or unsupported message!");
                    break;
            }
        }
    }

    public class Chest : MonoBehaviorJob<ChestData, ChestJob>
    {
    }
}
