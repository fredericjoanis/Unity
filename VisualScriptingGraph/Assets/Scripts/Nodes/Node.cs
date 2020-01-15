using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public abstract class Node : MonoBehaviour, IConvertGameObjectToEntity
{
    public abstract void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem);
}

public enum NodeTypeEnum
{
    Null,
    ConsoleLog,
    SetFloat,
    Start,
    Wait
}

public enum InputTypeEnum : System.UInt16
{
    Signal,
    Int,
    Float
}

[StructLayout(LayoutKind.Explicit)]
public struct InputTriggerValue : IComponentData
{
    [FieldOffset(0)]
    public InputTypeEnum inputTypeEnum;

    [FieldOffset(2)]
    public int intValue;

    [FieldOffset(2)]
    public float floatValue;
}

public struct NodeRuntime : IComponentData
{
    public delegate void Update(ref Entity nodeEntity, ref NodeRuntime nodeRuntime, ref VisualScriptingSystem.VisualScriptingExecution system);
    public delegate void InputTrigger(ref Entity inputTriggered, ref NodeRuntime nodeRuntime, ref InputTriggerValue inputTrigger, ref VisualScriptingSystem.VisualScriptingExecution system);

    public NodeTypeEnum NodeType;
    public int UpdateEachFrame;
    public FunctionPointer<Update> FunctionPointerUpdate;
    public FunctionPointer<InputTrigger> FunctionPointerInputTrigger;
}