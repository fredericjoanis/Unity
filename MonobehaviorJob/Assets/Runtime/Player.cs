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
    public struct PlayerData
    {
        public int totalMoney;
        public Unity.Mathematics.Random random;
    }

    public struct PlayerJob : IJobExecute<PlayerData>
    {
        public void Execute(ref JobArguments<PlayerData> args)
        {
            if (args.data.random.NextInt(0, 10) > 5)
            {
                args.SendMessage(Chest.Manager.Instance.Components[0].args.Id, new PickupChestMessage());
            }
        }

        public void ProcessMessage(ref JobArguments<PlayerData> args, IMessage message)
        {
            switch (message)
            {
                case MoneyTransaction money:
                    args.data.totalMoney += money.Amount;
                    break;
                default:
                    Debug.Assert(false, "Wrong Mailbox or unsupported message!");
                    break;
            }
        }
    }

    public class Player : MonoBehaviourJob<PlayerData, PlayerJob>
    {
        public override void Awake()
        {
            base.Awake();
            PlayerData playerData = Data[0];
            playerData.random.InitState(38);
            Data[0] = playerData;
        }
    }
}
