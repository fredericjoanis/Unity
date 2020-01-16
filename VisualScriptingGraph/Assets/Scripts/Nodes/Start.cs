using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public struct StartComponentData : IComponentData
{
    public Socket Output;
}

[BurstCompile]
public class StartFunctions
{
    [BurstCompile]
    public static unsafe void Initialize(ref NodeData nodeData, ref GraphContext graphContext)
    {
        graphContext.ProcessEachFrame();
    }

    [BurstCompile]
    public static void Update(ref NodeData nodeData, ref GraphContext graphContext)
    {
        graphContext.OutputTrigger(ref nodeData.StartComponentData.Output);
        graphContext.StopProcessEachFrame();
    }

    [BurstCompile]
    public static void GetNodeType(ref NodeTypeEnum nodeType)
    {
        nodeType = NodeTypeEnum.Wait;
    }
}

[RequiresEntityConversion]
public class Start : Node
{
    public SocketOutputSignal Output;

    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem, Entity nodeEntity)
    {
        StartComponentData componentData = new StartComponentData()
        {
            Output = Output.ConvertToSocketRuntime(nodeEntity, entity)
        };
        
        dstManager.AddComponentData(entity, new NodeRuntime()
        {
            NodeType = NodeTypeEnum.Start,
            FunctionPointerInitialize = BurstCompiler.CompileFunctionPointer<NodeRuntime.Initialize>(StartFunctions.Initialize),
            FunctionPointerUpdate = BurstCompiler.CompileFunctionPointer<NodeRuntime.Update>(StartFunctions.Update),
            NodeData = new NodeData() { StartComponentData = componentData }
        });
    }
}