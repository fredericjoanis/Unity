using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public abstract class Node : MonoBehaviour
{
    public abstract void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem, Entity nodeEntity);
}

public enum NodeTypeEnum
{
    Undefined,
    ConsoleLog,
    SetFloat,
    Start,
    Wait
}

public struct NodeRuntime : IComponentData
{
    public delegate void Initialize(ref Entity nodeEntity, ref VisualScriptingSystem.VisualScriptingExecution system);
    public delegate void Update(ref Entity nodeEntity, ref VisualScriptingSystem.VisualScriptingExecution system);
    public delegate void InputTrigger(ref TriggerData socketValue, ref VisualScriptingSystem.VisualScriptingExecution system);

    public NodeTypeEnum NodeType;
    public FunctionPointer<Initialize> FunctionPointerInitialize;
    public FunctionPointer<Update> FunctionPointerUpdate;
    public FunctionPointer<InputTrigger> FunctionPointerInputTrigger;
}