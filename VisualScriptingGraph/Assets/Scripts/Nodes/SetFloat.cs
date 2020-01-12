using Unity.Entities;

[RequiresEntityConversion]
public class SetFloat : Node
{
    public SocketInputSignal Trigger;
    public SocketInputFloat Value;
    public SocketOutputSignal Output;

    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
    }
}