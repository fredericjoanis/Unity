using System;
using Unity.Entities;
using UnityEngine;


[Serializable]
public struct EdgeRuntime : IComponentData
{
    public Entity FromNode;
    public Entity ToNode;
}

[Serializable]
public struct EdgeFloat : IComponentData
{
    public float value;
}


[Serializable]
[RequiresEntityConversion]
public class Edge : MonoBehaviour, IConvertGameObjectToEntity
{
    public SocketInput SocketInput;
    public SocketOutput SocketOutput;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {

    }
}