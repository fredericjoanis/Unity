using Prototype;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Prototype
{
    public enum MessageState
    {
        NotProcessed,
        Processed
    }

    public class MonoBehaviourJob<TData> : MonoBehaviour
    {
        public TData InitialData;

        public virtual void Awake()
        {
            Manager<TData, MonoBehaviourJob<TData>>.Instance.AddComponent(this);
        }

        void Update()
        {
        }

        public MessageState ProcessMessage(ref TData data, Message message)
        {
            return MessageState.NotProcessed;
        }

        private void OnDestroy()
        {
            Manager<TData, MonoBehaviourJob<TData>>.Instance.RemoveComponent(this);
        }
    }
}