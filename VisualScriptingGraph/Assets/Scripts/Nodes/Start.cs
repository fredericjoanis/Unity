using Unity.Entities;

[RequiresEntityConversion]
public class Start : Node
{
    public SocketOutputSignal Output;

    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
    }
}