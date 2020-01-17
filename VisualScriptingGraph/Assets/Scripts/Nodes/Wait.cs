using Unity.Burst;
using Unity.Entities;

// Thoughts
// The FunctionPointer limited to static functions is really limitating. More precisely, that we can only pass Blittable data.
//  a. In the current implementation, the nodes can't have access to ComponentDataFromEntity, NativeArrays, etc.
//  b. Which means nodes should be System to set any required context. Compositor is that way. Which probably means codegen is necessary.
//  d. IComponentData should be normal ComponentData, but because of FunctionPointer with only blittable data, 
//     I had to fudge them all in a single struct.
//  e. Some way to have indirections in jobs would make our life much easier.

public struct WaitComponentData : IComponentData
{
    public float WaitTime;
    public float TriggeredTime;
    public float CurrentTime;
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