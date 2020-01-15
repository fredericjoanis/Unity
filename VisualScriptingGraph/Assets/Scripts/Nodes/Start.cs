using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public struct StartComponentData : IComponentData
{
    public SocketRuntime Output;
}

[BurstCompile]
public class StartSystem : JobComponentSystem
{
    [NativeDisableParallelForRestrictionAttribute]
    private static ComponentDataFromEntity<StartComponentData> StartComponents;

    protected override void OnCreate()
    {
        StartComponents = GetComponentDataFromEntity<StartComponentData>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return inputDeps;
    }

    [BurstCompile]
    public static void Initialize(ref Entity entity, ref VisualScriptingSystem.VisualScriptingExecution system)
    {
        system.ProcessEachFrame(ref entity);
    }

    [BurstCompile]
    public static void Update(ref Entity entity, ref VisualScriptingSystem.VisualScriptingExecution system)
    {
        StartComponentData startData = StartComponents[entity];
        system.OutputTrigger(ref startData.Output);
        system.StopProcessEachFrame(ref entity);
    }
}

[RequiresEntityConversion]
public class Start : Node
{
    public SocketOutputSignal Output;

    [NativeDisableParallelForRestrictionAttribute]
    private static ComponentDataFromEntity<StartComponentData> StartComponents;

    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new NodeRuntime()
        {
            NodeType = NodeTypeEnum.Start,
            FunctionPointerInitialize = BurstCompiler.CompileFunctionPointer<NodeRuntime.Initialize>(StartSystem.Initialize),
            FunctionPointerUpdate = BurstCompiler.CompileFunctionPointer<NodeRuntime.Update>(StartSystem.Update),
        });

        dstManager.AddComponentData(entity, new StartComponentData()
        {
            Output = new SocketRuntime() { SocketEntity = dstManager.CreateEntity() }
        });
    }
}