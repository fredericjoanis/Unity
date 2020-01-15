using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public struct VisualScriptingGraphTag : IComponentData
{
}

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class VisualScriptingGraph : MonoBehaviour, IConvertGameObjectToEntity
{
    public List<Node> nodes;
    public List<Edge> edges;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        /*
        foreach(var node in nodes)
        {
            Entity entityNode = conversionSystem.EntityManager.CreateEntity();
            node.Convert(entityNode, dstManager, conversionSystem);
        }

        foreach (var edge in edges)
        {
            Entity entityEdge = conversionSystem.EntityManager.CreateEntity();
            edge.Convert(entityEdge, dstManager, conversionSystem);
        }
        */
    }
}

