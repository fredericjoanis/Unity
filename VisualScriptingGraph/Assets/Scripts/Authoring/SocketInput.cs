using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public abstract class SocketInput : MonoBehaviour, IConvertGameObjectToEntity
{
    public abstract void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem);
}
