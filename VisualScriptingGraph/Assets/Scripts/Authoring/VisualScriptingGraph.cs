using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public struct VisualScriptingGraphTag : IComponentData
{
    public Entity Entity; // ArchetypeChunk.GetNativeArray<VisualScriptingGraphTag> cannot be called on zero-sized IComponentData
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
            if(node)
            {
                Entity entityNode = conversionSystem.CreateAdditionalEntity(this);
                node.Convert(entity, dstManager, conversionSystem, entityNode);
                dstManager.AddSharedComponentData(entityNode, new NodeSharedComponentData { Graph = entity });
            }
        }

        foreach (var edge in edges)
        {
            if (edge)
            {
                Entity entityEdge = conversionSystem.CreateAdditionalEntity(this);
                edge.Convert(entityEdge, dstManager, conversionSystem);
                dstManager.AddSharedComponentData(entityEdge, new NodeSharedComponentData { Graph = entity });
            }
        }

        dstManager.AddComponentData(entity, new VisualScriptingGraphTag() { Entity = entity });
    }
}

/*
[UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
public class ConvertSystem : GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
        var graphs = GetEntityQuery(typeof(VisualScriptingGraph))
            .ToComponentArray<VisualScriptingGraph>();
    }
}
*/