using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public struct VisualScriptingGraphTag : IComponentData
{
}

public struct NodeSharedComponentData : ISharedComponentData
{
    public Entity Graph;
}

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class VisualScriptingGraph : MonoBehaviour, IConvertGameObjectToEntity
{
    public List<Node> nodes;
    public List<Edge> edges;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        foreach(var node in nodes)
        {
            Entity entityNode = conversionSystem.EntityManager.CreateEntity();
            node.Convert(entityNode, dstManager, conversionSystem, entity);
            dstManager.AddSharedComponentData(entityNode, new NodeSharedComponentData { Graph = entity });
        }

        foreach (var edge in edges)
        {
            Entity entityEdge = conversionSystem.EntityManager.CreateEntity();
            edge.Convert(entityEdge, dstManager, conversionSystem);
        }

        dstManager.AddComponentData(entity, new VisualScriptingGraphTag());
    }
}

