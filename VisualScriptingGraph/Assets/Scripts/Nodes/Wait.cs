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
public class WaitSystem : JobComponentSystem
{
    [NativeDisableParallelForRestrictionAttribute]
    private static ComponentDataFromEntity<WaitComponentData> WaitComponents;

    [BurstCompile]
    public static void Update(ref Entity entity, ref VisualScriptingSystem.VisualScriptingExecution system)
    {

    }

    [BurstCompile]
    public static void InputTrigger(ref TriggerData socketValue, ref VisualScriptingSystem.VisualScriptingExecution system)
    {
    }

    protected override void OnCreate()
    {
        WaitComponents = GetComponentDataFromEntity<WaitComponentData>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return inputDeps;
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
        dstManager.AddComponentData(entity, new NodeRuntime()
        {
            NodeType = NodeTypeEnum.Wait,
            FunctionPointerUpdate = BurstCompiler.CompileFunctionPointer<NodeRuntime.Update>(WaitSystem.Update),
            FunctionPointerInputTrigger = BurstCompiler.CompileFunctionPointer<NodeRuntime.InputTrigger>(WaitSystem.InputTrigger),
        });

        dstManager.AddComponentData(entity, new WaitComponentData()
        {
            TriggeredTime = 0,
            WaitTime = WaitTime.DefaultValue,
        });
    }
}