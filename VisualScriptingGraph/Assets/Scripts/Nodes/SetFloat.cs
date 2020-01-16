using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public struct SetFloatComponentData : IComponentData
{
    public float value;
}


[BurstCompile]
public class SetFloatFunctions
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
        nodeType = NodeTypeEnum.SetFloat;
    }
}

[RequiresEntityConversion]
public class SetFloat : Node
{
    public SocketInputSignal Trigger;
    public SocketInputFloat Value;
    public SocketOutputFloat Output;

    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem, Entity nodeEntity)
    {
        SetFloatComponentData componentData = new SetFloatComponentData() { value = Value.DefaultValue };
        
        dstManager.AddComponentData(entity, new NodeRuntime()
        {
            NodeType = NodeTypeEnum.SetFloat,
            FunctionPointerGetNodeType = BurstCompiler.CompileFunctionPointer<NodeRuntime.GetNodeType>(WaitFunctions.GetNodeType),
            FunctionPointerUpdate = BurstCompiler.CompileFunctionPointer<NodeRuntime.Update>(SetFloatFunctions.Update),
            FunctionPointerInputTrigger = BurstCompiler.CompileFunctionPointer<NodeRuntime.InputTrigger>(SetFloatFunctions.InputTrigger),
            NodeData = new NodeData() {SetFloatComponentData = componentData }
        });
        
    }
}