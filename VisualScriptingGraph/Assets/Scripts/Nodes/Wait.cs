using Unity.Entities;

[RequiresEntityConversion]
public class Wait : Node
{
    public SocketInputSignal Trigger;
    public SocketInputFloat WaitTime;
    public SocketOutputSignal Output;

    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
    }
}