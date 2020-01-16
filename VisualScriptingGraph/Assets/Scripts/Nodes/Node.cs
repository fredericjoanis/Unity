using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public abstract class Node : MonoBehaviour
{
    public abstract void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem, Entity nodeEntity);
}

// Need to be auto-generated
public enum NodeTypeEnum
{
    Undefined,
    ConsoleLog,
    SetFloat,
    Start,
    Wait
}

[StructLayout(LayoutKind.Explicit)]
public struct NodeData
{
    [FieldOffset(0)]
    public ConsoleLogComponentData ConsoleLogComponentData;
    [FieldOffset(0)]
    public SetFloatComponentData SetFloatComponentData;
    [FieldOffset(0)]
    public StartComponentData StartComponentData;
    [FieldOffset(0)]
    public WaitComponentData WaitComponentData;
}

public struct NodeRuntime : IComponentData
{
    // NativeArray prevent burst and unsafe C# to pass a ref or *
    public delegate void Initialize(ref NodeData nodeData, ref GraphContext graphContext);
    public delegate void Update(ref NodeData nodeData, ref GraphContext graphContext);
    public delegate void InputTrigger(ref NodeData nodeData, ref TriggerData triggerData, ref GraphContext graphContext);
    public delegate void GetNodeType(ref NodeTypeEnum nodeType);

    public NodeTypeEnum NodeType;
    public FunctionPointer<Initialize> FunctionPointerInitialize;
    public FunctionPointer<Update> FunctionPointerUpdate;
    public FunctionPointer<InputTrigger> FunctionPointerInputTrigger;
    public FunctionPointer<GetNodeType> FunctionPointerGetNodeType;

    public NodeData NodeData;
}