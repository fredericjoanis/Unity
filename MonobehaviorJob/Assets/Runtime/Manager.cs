using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Prototype
{
    public abstract class Manager<TData, TMonoBehaviourJob> 
        : MonoBehaviour
        where TMonoBehaviourJob : MonoBehaviourJob<TData>
    {
        public List<MonoBehaviourJob<TData>> Components = new List<MonoBehaviourJob<TData>>(10);
        public NativeList<MonoBehaviourJobData> Data;

        public static Manager<TData, TMonoBehaviourJob> Instance;

        JobHandle jobHandle;
        ManagerJob _managerJob = new ManagerJob();

        public struct MessageToSend
        {
            public Guid SendToGuid;
            public Message message;
        }

        public struct MonoBehaviourJobData
        {
            public TData UserData;
            public Guid guid;
            [ReadOnly] public NativeList<Message> messagesReceived;
            [WriteOnly] public NativeList<MessageToSend> messagesToSend;
        }

        public void Awake()
        {
            Data = new NativeList<MonoBehaviourJobData>(Allocator.Persistent);
            _managerJob.MonoBehaviorJobDataCollection = Data;
        }

        public void OnDestroy()
        {
            Data.Dispose();
        }

        public int AddComponent(MonoBehaviourJob<TData> monoBehaviorJob)
        {
            Components.Add(monoBehaviorJob);
            Guid guidToAdd = Guid.NewGuid();
            Data.Add(new MonoBehaviourJobData()
            {
                guid = guidToAdd, UserData = monoBehaviorJob.InitialData
            });
            Mailbox.Instance.messages.Add(guidToAdd, new List<Message>());
            return Components.Count - 1;
        }

        public void RemoveComponent(MonoBehaviourJob<TData> monoBehaviorJob)
        {
            int index = Components.IndexOf(monoBehaviorJob);
            Mailbox.Instance.messages.Remove(Data[index].guid);
            Components.RemoveAtSwapBack(index);
            Data.RemoveAtSwapBack(index);
            
        }

        public void Update()
        {
            for (int componentIndex = 0; componentIndex < Data.Length; componentIndex++)
            {
                MonoBehaviourJobData currentData = Data[componentIndex];
                List<Message> messagesReceived = Mailbox.Instance.messages[currentData.guid];

                currentData.messagesReceived = new NativeList<Message>(messagesReceived.Count, Allocator.TempJob);
                currentData.messagesToSend = new NativeList<MessageToSend>(Allocator.TempJob);

                for (int messageIndex = 0; messageIndex < messagesReceived.Count; messageIndex++)
                {
                    MessageState messageState = Components[componentIndex].ProcessMessage(ref currentData.UserData, messagesReceived[messageIndex]);
                    if (messageState == MessageState.NotProcessed)
                    {
                        currentData.messagesReceived.Add(messagesReceived[messageIndex]);
                    }
                }
                Data[componentIndex] = currentData;
            }
            jobHandle = _managerJob.Schedule();
        }

        public void LateUpdate()
        {
            jobHandle.Complete();
            for(int i = 0; i < Data.Length; i++)
            {
                MonoBehaviourJobData currentData = Data[i];
                
                for (int messageIndex = 0; messageIndex < currentData.messagesToSend.Length; messageIndex++)
                {
                    Mailbox.Instance.messages[currentData.messagesToSend[messageIndex].SendToGuid].Add(currentData.messagesToSend[messageIndex].message);
                }
                currentData.messagesReceived.Dispose();
                currentData.messagesToSend.Dispose();
            }
        }

        public abstract void Update(ref MonoBehaviourJobData monoBehaviorJobData);

        public abstract void ProcessMessage(ref MonoBehaviourJobData monoBehaviorJobData, Message message);

        public void SendMessage(ref MonoBehaviourJobData monoBehaviorJobData, Guid sendToGuid, Message message)
        {
            monoBehaviorJobData.messagesToSend.Add(new MessageToSend()
            {
                SendToGuid = sendToGuid,
                message = message
            });
        }

        public struct ManagerJob : IJob
        {
            [ReadOnly] public NativeList<MonoBehaviourJobData> MonoBehaviorJobDataCollection;

            public void Execute()
            {
                for (int i = 0; i < MonoBehaviorJobDataCollection.Length; i++)
                {
                    MonoBehaviourJobData monoBehaviorJobData = MonoBehaviorJobDataCollection[i];
                    for(int messageIndex = 0; messageIndex < monoBehaviorJobData.messagesReceived.Length; messageIndex++)
                    {
                        Instance.ProcessMessage(ref monoBehaviorJobData, monoBehaviorJobData.messagesReceived[i]);
                    }

                    Instance.Update(ref monoBehaviorJobData);
                    MonoBehaviorJobDataCollection[i] = monoBehaviorJobData;
                }
            }
        }
    }

    public class Mailbox
    {
        public static readonly Mailbox Instance = new Mailbox();
        public Dictionary<Guid, List<Message>> messages = new Dictionary<Guid, List<Message>>();

        private Mailbox()
        {

        }
    }
}