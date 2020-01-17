using System;
using Unity.Burst;
using Unity.Entities;

public struct ConsoleLogComponentData : IComponentData
{
}

[BurstCompile]
public class ConsoleLogFunctions
{
    [BurstCompile]
    public static void Update(ref NodeData nodeData, ref NodeRuntime nodeRuntime, ref GraphContext graphContext)
    {

    }

    [BurstCompile]
    public static void InputTrigger(ref NodeData nodeData, ref TriggerData socket, ref GraphContext graphContext)
    {
    }

    [BurstCompile]
    public static void GetNodeType(ref NodeTypeEnum nodeType)
    {
        nodeType = NodeTypeEnum.ConsoleLog;
    }
}

[RequiresEntityConversion]
public class ConsoleLog : Node
{
    public SocketInputSignal Trigger;
    public SocketInputString String;
    public SocketOutputSignal Output;

    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem, Entity nodeEntity)
    {
        ConsoleLogComponentData componentData = new ConsoleLogComponentData() { };
        
        dstManager.AddComponentData(entity, new NodeRuntime()
        {
            NodeType = NodeTypeEnum.ConsoleLog,
            FunctionPointerInputTrigger = BurstCompiler.CompileFunctionPointer<NodeRuntime.InputTrigger>(ConsoleLogFunctions.InputTrigger),
            NodeData = new NodeData() { ConsoleLogComponentData = componentData }
        });
    }
}