using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public abstract class SocketOutput : MonoBehaviour, IConvertGameObjectToEntity
{
    public abstract void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem);
}