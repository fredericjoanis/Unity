using Unity.Entities;

public struct ConsoleLogComponentData : IComponentData
{
}

[RequiresEntityConversion]
public class ConsoleLog : Node
{
    public SocketInputSignal Trigger;
    public SocketInputString String;
    public SocketOutputSignal Output;

    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
    }
}