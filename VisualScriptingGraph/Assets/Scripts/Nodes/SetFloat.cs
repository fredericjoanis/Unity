using Unity.Entities;

public struct SetFloatComponentData : IComponentData
{
    public float value;

    public static void SetFloatTrigger(ref SetFloatComponentData data, EdgeFloat edge)
    {
        data.value = edge.value;
    }

    public static void SetFloatSignal(ref SetFloatComponentData data, Edge edge)
    {
        Execute(ref data);
    }

    public static void Execute(ref SetFloatComponentData data)
    {

    }
}

[RequiresEntityConversion]
public class SetFloat : Node
{
    public SocketInputSignal Trigger;
    public SocketInputFloat Value;
    public SocketOutputFloat Output;

    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        Entity socketTrigger = dstManager.CreateEntity();
        Entity socketValue = dstManager.CreateEntity();
        Entity socketOutput = dstManager.CreateEntity();

        dstManager.AddComponentData(entity, new SetFloatComponentData() { value = Value.DefaultValue });
    }
}