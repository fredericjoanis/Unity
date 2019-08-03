using Unity.Burst;
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
        public void Execute(ref JobProcessingArgs<PlayerData> args)
        {
            if (args.data.random.NextInt(0, 10) > 5)
            {
                args.SendMessage(Chest.Manager.Instance.Components[0].args.Id, new PickupChestMessage());
            }
        }

        public void ProcessMessage(ref JobProcessingArgs<PlayerData> args, IMessage message)
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

    public class Player : MonoBehaviorJob<PlayerData, PlayerJob>
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
