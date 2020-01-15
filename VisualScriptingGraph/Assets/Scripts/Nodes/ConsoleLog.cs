using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public struct ConsoleLogComponentData : IComponentData
{
}

[BurstCompile]
public class ConsoleLogSystem : JobComponentSystem
{
    [NativeDisableParallelForRestrictionAttribute]
    private static ComponentDataFromEntity<ConsoleLogComponentData> ConsoleLogComponents;

    [BurstCompile]
    public static void Update(ref Entity entity, ref NodeRuntime nodeRuntime, ref VisualScriptingSystem.VisualScriptingExecution system)
    {

    }

    [BurstCompile]
    public static void InputTrigger(ref TriggerData socket, ref VisualScriptingSystem.VisualScriptingExecution system)
    {
    }

    protected override void OnCreate()
    {
        ConsoleLogComponents = GetComponentDataFromEntity<ConsoleLogComponentData>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return inputDeps;
    }
}

[RequiresEntityConversion]
public class ConsoleLog : Node
{
    public SocketInputSignal Trigger;
    public SocketInputString String;
    public SocketOutputSignal Output;

    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new NodeRuntime()
        {
            NodeType = NodeTypeEnum.ConsoleLog,
            FunctionPointerInputTrigger = BurstCompiler.CompileFunctionPointer<NodeRuntime.InputTrigger>(ConsoleLogSystem.InputTrigger),
        });
        dstManager.AddComponentData(entity, new ConsoleLogComponentData() { });
    }
}