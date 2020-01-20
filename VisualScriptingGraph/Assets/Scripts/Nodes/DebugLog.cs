using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public struct DebugLogComponentData : IComponentData
{
    public Entity TriggerSocket;
    public Entity OutputSocket;

    public BlobAssetReference<BlobString> StrToOutput;
    public int HasTriggered;
}

public struct DebugLogJob : INodeJob
{
    [NativeDisableParallelForRestriction]
    public ComponentDataFromEntity<DebugLogComponentData> DebugLogComponentData;

    public void Execute(Entity node, ref VisualScriptingSystem.VisualScriptingGraphJob graph)
    {
    }

    public void InputTriggered(Entity node, ref TriggerData triggerData, ref VisualScriptingSystem.VisualScriptingGraphJob graph)
    {
        DebugLogComponentData data = DebugLogComponentData[node];
        data.HasTriggered = 1;
        DebugLogComponentData[node] = data;
    }

    public void OnStartRunning(Entity node, ref VisualScriptingSystem.VisualScriptingGraphJob graph)
    {
    }
}

public class DebugLogSystem : INodeSystem
{
    public static DebugLogJob DebugLogJob;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        DebugLogJob.DebugLogComponentData = GetComponentDataFromEntity<DebugLogComponentData>();

        Entities.ForEach((ref DebugLogComponentData data) =>
        {
            if (data.HasTriggered > 0)
            {
                Debug.Log(data.StrToOutput.Value.ToString());
                data.HasTriggered = 0;
            }
        }).WithoutBurst().Run();

        return inputDeps;
    }
}

[RequiresEntityConversion]
public class DebugLog : Node
{
    public SocketInputSignal Trigger;
    public SocketOutputSignal Output;
    public string DefaultMessage;

    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);
        ref BlobString blobStr = ref blobBuilder.ConstructRoot<BlobString>();

        blobBuilder.AllocateString(ref blobStr, DefaultMessage);

        dstManager.AddComponentData(entity, new DebugLogComponentData()
        {
            StrToOutput = blobBuilder.CreateBlobAssetReference<BlobString>(Allocator.Persistent)
        });

        dstManager.AddComponentData(entity, new NodeType()
        {
            Value = NodeTypeEnum.DebugLog,
        });

        blobBuilder.Dispose();
    }
}