using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[UpdateBefore(typeof(VisualScriptingSystem))]
public abstract class INodeSystem : JobComponentSystem
{
}

public interface INodeJob
{
    void Initialize(Entity node, ref VisualScriptingSystem.VisualScriptingGraphJob graph);
    void Execute(Entity node, ref VisualScriptingSystem.VisualScriptingGraphJob graph);
    void InputTriggered(Entity node, ref TriggerData triggerData, ref VisualScriptingSystem.VisualScriptingGraphJob graph);
}

[RequiresEntityConversion]
public abstract class Node : MonoBehaviour, IConvertGameObjectToEntity
{
    public abstract void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem);
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

public struct NodeType : IComponentData
{
    public NodeTypeEnum Value;
}