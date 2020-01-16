using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public struct WaitComponentData : IComponentData
{
    public float WaitTime;
    public float TriggeredTime;
}

[BurstCompile]
public class WaitFunctions
{
    [BurstCompile]
    public static void Update(ref NodeData nodeData, ref GraphContext graphContext)
    {

    }

    [BurstCompile]
    public static void InputTrigger(ref NodeData nodeData, ref TriggerData socketValue, ref GraphContext graphContext)
    {
    }

    [BurstCompile]
    public static void GetNodeType(ref NodeTypeEnum nodeType)
    {
        nodeType = NodeTypeEnum.Wait;
    }
}

[RequiresEntityConversion]
public class Wait : Node
{
    public SocketInputSignal Trigger;
    public SocketInputFloat WaitTime;
    public SocketOutputSignal Output;

    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem, Entity NodeEntity)
    {
        WaitComponentData componentData = new WaitComponentData()
        {
            TriggeredTime = 0,
            WaitTime = WaitTime.DefaultValue,
        };
        
        dstManager.AddComponentData(entity, new NodeRuntime()
        {
            NodeType = NodeTypeEnum.Wait,
            FunctionPointerGetNodeType = BurstCompiler.CompileFunctionPointer<NodeRuntime.GetNodeType>(WaitFunctions.GetNodeType),
            FunctionPointerUpdate = BurstCompiler.CompileFunctionPointer<NodeRuntime.Update>(WaitFunctions.Update),
            FunctionPointerInputTrigger = BurstCompiler.CompileFunctionPointer<NodeRuntime.InputTrigger>(WaitFunctions.InputTrigger),
            NodeData = new NodeData() { WaitComponentData = componentData }
        });
    }
}