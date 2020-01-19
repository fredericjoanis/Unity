using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public abstract class SocketInput : MonoBehaviour, IConvertGameObjectToEntity
{
    public abstract SocketType GetSocketType();

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData<Socket>(entity, new Socket()
        {
            SocketType = GetSocketType()
        });
    }
}
