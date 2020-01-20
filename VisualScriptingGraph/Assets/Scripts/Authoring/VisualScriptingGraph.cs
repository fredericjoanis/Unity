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

    protected void SetSocketNode(Entity socketEntity, Entity nodeEntity)
    {
        Socket socket = DstEntityManager.GetComponentData<Socket>(socketEntity);
        socket.NodeEntity = nodeEntity;
        DstEntityManager.SetComponentData<Socket>(socketEntity, socket);
    }

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
                        {
                            Entity socketOutputEntity = GetPrimaryEntity(start.Output);
                            SetSocketNode(socketOutputEntity, nodeEntity);

                            StartComponentData startData = DstEntityManager.GetComponentData<StartComponentData>(nodeEntity);
                            startData.OutputSocket = socketOutputEntity;
                            DstEntityManager.SetComponentData<StartComponentData>(nodeEntity, startData);
                        }
                    break;
                    case Wait wait:
                        {
                            Entity socketOutputEntity = GetPrimaryEntity(wait.Output);
                            SetSocketNode(socketOutputEntity, nodeEntity);
                            SetSocketNode(GetPrimaryEntity(wait.Trigger), nodeEntity);
                            SetSocketNode(GetPrimaryEntity(wait.WaitTime), nodeEntity);

                            WaitComponentData waitData = DstEntityManager.GetComponentData<WaitComponentData>(nodeEntity);
                            waitData.Output = socketOutputEntity;
                            DstEntityManager.SetComponentData<WaitComponentData>(nodeEntity, waitData);
                        }
                        break;
                    case DebugLog debugLog:
                        {
                            Entity socketOutputEntity = GetPrimaryEntity(debugLog.Output);
                            SetSocketNode(socketOutputEntity, nodeEntity);
                            SetSocketNode(GetPrimaryEntity(debugLog.Trigger), nodeEntity);

                            DebugLogComponentData debugLogComponentData = DstEntityManager.GetComponentData<DebugLogComponentData>(nodeEntity);
                            debugLogComponentData.OutputSocket = socketOutputEntity;
                            DstEntityManager.SetComponentData<DebugLogComponentData>(nodeEntity, debugLogComponentData);
                        }
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
