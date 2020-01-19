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
        dstManager.AddComponentData(entity, new VisualScriptingGraphTag() { Entity = entity });
    }
}


[UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
public class ConvertSystem : GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
        var graphs = GetEntityQuery(typeof(VisualScriptingGraph)).ToComponentArray<VisualScriptingGraph>();
        
        foreach (VisualScriptingGraph graph in graphs)
        {
            Entity graphEntity = GetPrimaryEntity(graph);
            foreach (Node node in graph.nodes)
            {
                Entity nodeEntity = GetPrimaryEntity(node);
                DstEntityManager.AddSharedComponentData(nodeEntity, new NodeSharedComponentData { Graph = graphEntity });

                // Code-generated start
                switch (node)
                {
                    case Start start:
                        var startData = DstEntityManager.GetComponentData<StartComponentData>(nodeEntity);
                        var socketOutputEntity = GetPrimaryEntity(start.Output);

                        var socket = DstEntityManager.GetComponentData<Socket>(socketOutputEntity);
                        socket.NodeEntity = nodeEntity;
                        
                        startData.OutputSocket = nodeEntity;

                        DstEntityManager.SetComponentData<Socket>(socketOutputEntity, socket);
                        DstEntityManager.SetComponentData<StartComponentData>(nodeEntity, startData);
                    break;
                }
                // Code-generated end
            }

            foreach (Edge edge in graph.edges)
            {
                Entity edgeEntity = GetPrimaryEntity(edge);
                DstEntityManager.AddSharedComponentData(edgeEntity, new NodeSharedComponentData { Graph = graphEntity });

                var edgeRuntime = DstEntityManager.GetComponentData<EdgeRuntime>(edgeEntity);
                edgeRuntime.SocketInput = GetPrimaryEntity(edge.SocketInput);
                edgeRuntime.SocketOutput = GetPrimaryEntity(edge.SocketOutput);
                DstEntityManager.SetComponentData<EdgeRuntime>(edgeEntity, edgeRuntime);
            }
        }
    }
}
