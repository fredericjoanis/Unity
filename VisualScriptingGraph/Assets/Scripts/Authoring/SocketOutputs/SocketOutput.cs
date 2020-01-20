using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public abstract class SocketOutput : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData<Socket>(entity,
        new Socket()
        {
            SocketType = SocketType.Undefined, // We don't care about output socket type
        });
    }
}
