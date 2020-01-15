using System;
using Unity.Entities;
using UnityEngine;

public struct EdgeRuntime : IComponentData
{
    public Socket SocketInput;
    public Socket SocketOutput;
}

[Serializable]
[RequiresEntityConversion]
public class Edge : MonoBehaviour
{
    public SocketInput SocketInput;
    public SocketOutput SocketOutput;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var Sockets = conversionSystem.GetComponentDataFromEntity<Socket>();

        dstManager.AddComponentData(entity, new EdgeRuntime()
        {
            SocketInput = SocketInput.Socket,
            SocketOutput = SocketOutput.Socket
        });
    }
}