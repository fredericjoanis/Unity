using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public class SocketOutputSignal : SocketOutput
{
    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        throw new System.NotImplementedException();
    }
}