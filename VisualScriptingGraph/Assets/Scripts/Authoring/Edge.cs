using System;
using Unity.Entities;
using UnityEngine;

public struct EdgeRuntime : IComponentData
{
    public Entity SocketInput;
    public Entity SocketOutput;
}

[Serializable]
[RequiresEntityConversion]
public class Edge : MonoBehaviour, IConvertGameObjectToEntity
{
    public SocketInput SocketInput;
    public SocketOutput SocketOutput;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new EdgeRuntime()
        {
        });
    }
}