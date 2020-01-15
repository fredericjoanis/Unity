using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public struct StartComponentData : IComponentData
{
}

[BurstCompile]
public class StartSystem : JobComponentSystem
{
    [NativeDisableParallelForRestrictionAttribute]
    private static ComponentDataFromEntity<StartComponentData> StartComponents;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        StartComponents = GetComponentDataFromEntity<StartComponentData>();
        return inputDeps;
    }

    [BurstCompile]
    public static void Update(ref Entity entity, ref NodeRuntime nodeRuntime, ref VisualScriptingSystem.VisualScriptingExecution system)
    {
        nodeRuntime.UpdateEachFrame = 0;
        // System.OutputTrigger(outputTrigger);
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
            UpdateEachFrame = 1,
            FunctionPointerUpdate = BurstCompiler.CompileFunctionPointer<NodeRuntime.Update>(StartSystem.Update),
        });
        dstManager.AddComponentData(entity, new StartComponentData() { });
    }
}