using System;
using Unity.Entities;
using UnityEngine;


public struct EdgeRuntime : IComponentData
{
    public SocketRuntime SocketInput;
    public SocketRuntime SocketOutput;
}

[Serializable]
[RequiresEntityConversion]
public class Edge : MonoBehaviour
{
    public SocketInput SocketInput;
    public SocketOutput SocketOutput;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var Sockets = conversionSystem.GetComponentDataFromEntity<SocketRuntime>();


        dstManager.AddComponentData(entity, new EdgeRuntime()
        {
           
        });
    }

}