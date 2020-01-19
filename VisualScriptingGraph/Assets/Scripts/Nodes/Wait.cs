using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public struct WaitComponentData : IComponentData
{
    public double WaitTime;
    public double TriggeredTime;
    public Entity Output;
}

public struct WaitJob : INodeJob
{
    [NativeDisableParallelForRestriction]
    public ComponentDataFromEntity<WaitComponentData> WaitComponentData;

    public double ElapsedTime;

    public void Execute(Entity node, ref VisualScriptingSystem.VisualScriptingGraphJob graph)
    {
        WaitComponentData data = WaitComponentData[node];
        if(data.TriggeredTime + data.WaitTime <= ElapsedTime)
        {
            graph.OutputSignal(data.Output);
            graph.StopProcessEachFrame(node);
        }
    }

    public void OnStartRunning(Entity node, ref VisualScriptingSystem.VisualScriptingGraphJob graph)
    {
    }

    public void InputTriggered(Entity node, ref TriggerData triggerData, ref VisualScriptingSystem.VisualScriptingGraphJob graph)
    {
        WaitComponentData data = WaitComponentData[node];
        data.TriggeredTime = ElapsedTime;
        WaitComponentData[node] = data;
        graph.ProcessEachFrame(node);
    }
}

public class WaitSystem : INodeSystem
{
    public static WaitJob WaitJob;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityQuery query = GetEntityQuery(typeof(WaitComponentData));

        WaitJob.WaitComponentData = GetComponentDataFromEntity<WaitComponentData>();
        WaitJob.ElapsedTime = Time.ElapsedTime;

        return inputDeps;
    }
}

[RequiresEntityConversion]
public class Wait : Node
{
    public SocketInputSignal Trigger;
    public SocketInputFloat WaitTime;
    public SocketOutputSignal Output;

    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new WaitComponentData()
        {
            TriggeredTime = 0,
            WaitTime = WaitTime.DefaultValue,
            //Output = Output.ConvertToSocketRuntime(nodeEntity, entity),
        });
        
        dstManager.AddComponentData(entity, new NodeType()
        {
            Value = NodeTypeEnum.Wait,
        });
    }
}
