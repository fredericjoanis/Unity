using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public class SocketInputFloat : SocketInput
{
    public float DefaultValue;

    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    { }
}
