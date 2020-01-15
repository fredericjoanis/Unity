using Unity.Entities;

[RequiresEntityConversion]
public class SocketInputSignal : SocketInput
{
    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    { }
}
