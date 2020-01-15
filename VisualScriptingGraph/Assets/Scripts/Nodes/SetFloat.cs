using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public struct SetFloatComponentData : IComponentData
{
    public float value;
}


[BurstCompile]
public class SetFloatSystem : JobComponentSystem
{
    [NativeDisableParallelForRestrictionAttribute]
    private static ComponentDataFromEntity<SetFloatComponentData> SetFloatComponents;

    [BurstCompile]
    public static void Update(ref Entity entity, ref VisualScriptingSystem.VisualScriptingExecution system)
    {

    }

    [BurstCompile]
    public static void InputTrigger(ref Entity inputTriggered, ref TriggerData socketValue, ref VisualScriptingSystem.VisualScriptingExecution system)
    {
    }

    protected override void OnCreate()
    {
        SetFloatComponents = GetComponentDataFromEntity<SetFloatComponentData>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return inputDeps;
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
        dstManager.AddComponentData(entity, new NodeRuntime()
        {
            NodeType = NodeTypeEnum.SetFloat,
            FunctionPointerUpdate = BurstCompiler.CompileFunctionPointer<NodeRuntime.Update>(SetFloatSystem.Update),
            //FunctionPointerInputTrigger = BurstCompiler.CompileFunctionPointer<NodeRuntime.InputTrigger>(SetFloatSystem.InputTrigger),
        });
        dstManager.AddComponentData(entity, new SetFloatComponentData() { value = Value.DefaultValue });
    }
}