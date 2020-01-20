using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

public struct StartComponentData : IComponentData
{
    public Entity OutputSocket;
}

public struct StartJob : INodeJob
{
    [NativeDisableContainerSafetyRestriction]
    public ComponentDataFromEntity<StartComponentData> StartComponentData;

    public void OnStartRunning(Entity node, ref VisualScriptingSystem.VisualScriptingGraphJob graph)
    {
        graph.ProcessEachFrame(node);
    }

    public void Execute(Entity node, ref VisualScriptingSystem.VisualScriptingGraphJob graph)
    {
        var data = StartComponentData[node];
        graph.OutputSignal(data.OutputSocket);
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

    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new StartComponentData()
        {
        });
        
        dstManager.AddComponentData(entity, new NodeType()
        {
            Value = NodeTypeEnum.Start,
        });
    }
}