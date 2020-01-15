using Unity.Entities;

public struct WaitComponentData : IComponentData
{
    public float WaitTime;
    public float TriggeredTime;
}

[RequiresEntityConversion]
public class Wait : Node
{
    public SocketInputSignal Trigger;
    public SocketInputFloat WaitTime;
    public SocketOutputSignal Output;

    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new WaitComponentData() { TriggeredTime = 0, WaitTime = WaitTime.DefaultValue });
    }
}