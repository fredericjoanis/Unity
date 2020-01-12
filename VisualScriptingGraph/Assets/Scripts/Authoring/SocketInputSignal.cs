using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public class SocketInputSignal : SocketInput
{
    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    { }
}
