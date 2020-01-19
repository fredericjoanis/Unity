using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public struct StartComponentData : IComponentData
{
    public Socket Output;
}

public struct StartJob : INodeJob
{
    [NativeDisableParallelForRestriction]
    public ComponentDataFromEntity<StartComponentData> StartComponentData;

    public void Initialize(Entity node, ref VisualScriptingSystem.VisualScriptingGraphJob graph)
    {
        graph.ProcessEachFrame(node);
    }

    public void Execute(Entity node, ref VisualScriptingSystem.VisualScriptingGraphJob graph)
    {
        var data = StartComponentData[node];
        graph.OutputSignal(ref data.Output);
        graph.StopProcessEachFrame(node);
    }

    public void InputTriggered(Entity node, ref TriggerData triggerData, ref VisualScriptingSystem.VisualScriptingGraphJob graph)
    {
    }
}

public class StartSystem : INodeSystem
{
    public static StartJob StartJob;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        StartJob.StartComponentData = GetComponentDataFromEntity<StartComponentData>();
        return inputDeps;
    }
}

[RequiresEntityConversion]
public class Start : Node
{
    public SocketOutputSignal Output;

    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem, Entity nodeEntity)
    {
        dstManager.AddComponentData(entity, new StartComponentData()
        {
            Output = Output.ConvertToSocketRuntime(nodeEntity, entity)
        });
        
        dstManager.AddComponentData(entity, new NodeType()
        {
            Value = NodeTypeEnum.Start,
        });
    }
}