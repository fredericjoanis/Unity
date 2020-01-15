using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public class SocketInputString : SocketInput
{
    public string DefaultString;

    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    { }
}
