using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public struct GraphContext
{
    public int _ProcessEachFrame;
    public int _StopProcessEachFrame;

    public int _TriggerOutput;
    public TriggerData TriggerData;
}

[BurstCompile]
public static class GraphContextExt
{
    [BurstCompile]
    public static void ProcessEachFrame(ref GraphContext graphContext)
    {
        graphContext._ProcessEachFrame = 1;
    }

    [BurstCompile]
    public static void StopProcessEachFrame(ref GraphContext graphContext)
    {
        graphContext._StopProcessEachFrame = 1;
    }

    [BurstCompile]
    public static void OutputSignal(ref GraphContext graphContext, ref Socket socket)
    {
        graphContext._TriggerOutput = 1;
        graphContext.TriggerData.Socket = socket;
    }

    [BurstCompile]
    public static void OutputFloat(ref GraphContext graphContext, ref Socket socket, ref float value)
    {
        graphContext._TriggerOutput = 1;
        graphContext.TriggerData.Socket = socket;
        if (graphContext.TriggerData.Socket.SocketType != SocketType.Float)
        {
            // More Error handling here
            graphContext.TriggerData.Socket.SocketType = SocketType.Float;
        }
        graphContext.TriggerData.FloatValue = value;
    }

    [BurstCompile]
    public static void OutputInt(ref GraphContext graphContext, ref Socket socket, ref int value)
    {
        graphContext._TriggerOutput = 1;
        graphContext.TriggerData.Socket = socket;
        if (graphContext.TriggerData.Socket.SocketType != SocketType.Int)
        {
            // More Error handling here
            graphContext.TriggerData.Socket.SocketType = SocketType.Int;
        }
        graphContext.TriggerData.IntValue = value;
    }

    [BurstCompile]
    public static void OutputVector2(ref GraphContext graphContext, ref Socket socket, ref Vector2 value)
    {
        graphContext._TriggerOutput = 1;
        graphContext.TriggerData.Socket = socket;
        if (graphContext.TriggerData.Socket.SocketType != SocketType.Vector2)
        {
            // More Error handling here
            graphContext.TriggerData.Socket.SocketType = SocketType.Vector2;
        }
        graphContext.TriggerData.Vector2 = value;
    }

    [BurstCompile]
    public static void OutputVector3(ref GraphContext graphContext, ref Socket socket, ref Vector3 value)
    {
        graphContext._TriggerOutput = 1;
        graphContext.TriggerData.Socket = socket;
        if (graphContext.TriggerData.Socket.SocketType != SocketType.Vector3)
        {
            // More Error handling here
            graphContext.TriggerData.Socket.SocketType = SocketType.Vector3;
        }
        graphContext.TriggerData.Vector3 = value;
    }

    [BurstCompile]
    public static void OutputVector4(ref GraphContext graphContext, ref Socket socket, ref Vector4 value)
    {
        graphContext._TriggerOutput = 1;
        graphContext.TriggerData.Socket = socket;
        if (graphContext.TriggerData.Socket.SocketType != SocketType.Vector4)
        {
            // More Error handling here
            graphContext.TriggerData.Socket.SocketType = SocketType.Vector4;
        }
        graphContext.TriggerData.Vector4 = value;
    }
}

[BurstCompile]
public class VisualScriptingSystem : JobComponentSystem
{
    [BurstCompile]
    public struct VisualScriptingGraphJob : IJob
    {
        public Entity GraphEntity;

        [ReadOnly] public NativeArray<NodeRuntime> NodesInGraph;
        [ReadOnly] public NativeArray<EdgeRuntime> EdgesInGraph;
        [ReadOnly] public NativeArray<Entity> NodeEntities;
        [ReadOnly] public NativeArray<Entity> EdgeEntities;

        [NativeDisableParallelForRestrictionAttribute]
        public ComponentDataFromEntity<NodeRuntime> NodeRuntime;

        [NativeDisableParallelForRestrictionAttribute]
        [ReadOnly] public NativeMultiHashMap<Entity, Entity> OutputsToEdgesEntity;

        [NativeDisableParallelForRestrictionAttribute]
        public ComponentDataFromEntity<EdgeRuntime> EdgeRuntime;

        private NativeList<Entity> ProcessThisFrame;
        private NativeList<Entity> ProcessToAdd;
        private NativeList<Entity> ProcessToRemove;

        private NativeList<TriggerData> InputsToTriggerSignal;
        private NativeList<TriggerData> InputsToTrigger;

        // The entity key is the External Graph
        public NativeMultiHashMap<Entity, TriggerData> ExternalInputs;
        
        public NativeArray<NodeData> NodeData;

        public void Initialize()
        {
            ProcessThisFrame = new NativeList<Entity>(Allocator.Persistent);
            ProcessToAdd = new NativeList<Entity>(Allocator.Persistent);
            ProcessToRemove = new NativeList<Entity>(Allocator.Persistent);
            InputsToTriggerSignal = new NativeList<TriggerData>(Allocator.Persistent);
            InputsToTrigger = new NativeList<TriggerData>(Allocator.Persistent);
            ExternalInputs = new NativeMultiHashMap<Entity, TriggerData>();
            OutputsToEdgesEntity = new NativeMultiHashMap<Entity, Entity>();

            for (int i = 0; i < NodesInGraph.Length; i++)
            {
                Entity nodeEntity = NodeEntities[i];
                GraphContext graphContext = new GraphContext();
                NodeRuntime nodeRuntime = NodeRuntime[nodeEntity];
                NodesInGraph[i].FunctionPointerInitialize.Invoke(ref nodeRuntime.NodeData, ref graphContext);
                NodeRuntime[nodeEntity] = nodeRuntime;
                ProcessGraphContext(ref nodeEntity, ref graphContext);
            }

            if(ProcessToAdd.Length > 0)
            {
                ProcessThisFrame.CopyFrom(ProcessToAdd.ToArray());
                ProcessToAdd.Clear();
            }

            for (int i = 0; i < EdgesInGraph.Length; i++)
            {
                Entity edgeEntity = EdgeEntities[i];
                OutputsToEdgesEntity.Add(EdgesInGraph[i].SocketOutput.SocketEntity, edgeEntity);
            }
        }
        
        public void Dispose()
        {
            ProcessThisFrame.Dispose();
            ProcessToAdd.Dispose();
            ProcessToRemove.Dispose();
            InputsToTriggerSignal.Dispose();
            InputsToTrigger.Dispose();
            OutputsToEdgesEntity.Dispose();
        }

        public bool NeedProcessing()
        {
            return ProcessThisFrame.Length > 0 || InputsToTrigger.Length > 0 || InputsToTriggerSignal.Length > 0;
        }

        // Need to find a way to have direct function calls inside this class 
        // or direct access to arrays in the FunctionPointer functions.
        // This graph context is probably a bottleneck.
        public void ProcessGraphContext(ref Entity currentNode, ref GraphContext graphContext)
        {
            if (graphContext._ProcessEachFrame != 0)
            {
                ProcessToAdd.Add(currentNode);
            }

            if (graphContext._StopProcessEachFrame != 0)
            {
                ProcessToRemove.Add(currentNode);
            }

            if(graphContext._TriggerOutput != 0)
            {
                switch(graphContext.TriggerData.Socket.SocketType)
                {
                    case SocketType.Signal:
                        OutputSignal(ref graphContext.TriggerData.Socket);
                    break;
                    case SocketType.Int:
                        OutputInt(ref graphContext.TriggerData.Socket, graphContext.TriggerData.IntValue);
                        break;
                    case SocketType.Float:
                        OutputFloat(ref graphContext.TriggerData.Socket, graphContext.TriggerData.FloatValue);
                        break;
                    case SocketType.Vector2:
                        OutputVector2(ref graphContext.TriggerData.Socket, graphContext.TriggerData.Vector2);
                        break;
                    case SocketType.Vector3:
                        OuputVector3(ref graphContext.TriggerData.Socket, graphContext.TriggerData.Vector3);
                        break;
                    case SocketType.Vector4:
                        OutputVector4(ref graphContext.TriggerData.Socket, graphContext.TriggerData.Vector4);
                        break;
                }
            }
        }

        [BurstCompile]
        public void OutputSignal(ref Socket socketOutput)
        {
            NativeArray<Entity> edgesEntities = OutputsToEdgesEntity.GetValueArray(Allocator.TempJob);
            for(int i = 0; i < edgesEntities.Length; i++)
            {
                EdgeRuntime edge = EdgeRuntime[edgesEntities[i]];
                
                TriggerData val = new TriggerData() { Socket = edge.SocketInput };
                InputsToTriggerSignal.Add(val);
            }
        }
        
        public void OutputFloat(ref Socket socketOutput, float value)
        {
            NativeArray<Entity> edgesEntities = OutputsToEdgesEntity.GetValueArray(Allocator.TempJob);
            for (int i = 0; i < edgesEntities.Length; i++)
            {
                EdgeRuntime edge = EdgeRuntime[edgesEntities[i]];

                TriggerData val = new TriggerData() { Socket = edge.SocketInput };
                ConvertTriggerData.ConvertDataFloat(ref val, ref value);

                InputsToTrigger.Add(val);
            }
        }

        public void OutputInt(ref Socket socketOutput, int value)
        {
            NativeArray<Entity> edgesEntities = OutputsToEdgesEntity.GetValueArray(Allocator.TempJob);
            for (int i = 0; i < edgesEntities.Length; i++)
            {
                EdgeRuntime edge = EdgeRuntime[edgesEntities[i]];

                TriggerData val = new TriggerData() { Socket = edge.SocketInput };
                ConvertTriggerData.ConvertDataInt(ref val, ref value);

                InputsToTrigger.Add(val);
            }
        }

        public void OutputVector2(ref Socket socketOutput, Vector2 value)
        {
            NativeArray<Entity> edgesEntities = OutputsToEdgesEntity.GetValueArray(Allocator.TempJob);
            for (int i = 0; i < edgesEntities.Length; i++)
            {
                EdgeRuntime edge = EdgeRuntime[edgesEntities[i]];

                TriggerData val = new TriggerData() { Socket = edge.SocketInput };
                ConvertTriggerData.ConvertDataVector2(ref val, ref value);

                InputsToTrigger.Add(val);
            }
        }

        public void OuputVector3(ref Socket socketOutput, Vector3 value)
        {
            NativeArray<Entity> edgesEntities = OutputsToEdgesEntity.GetValueArray(Allocator.TempJob);
            for (int i = 0; i < edgesEntities.Length; i++)
            {
                EdgeRuntime edge = EdgeRuntime[edgesEntities[i]];

                TriggerData val = new TriggerData() { Socket = edge.SocketInput };
                ConvertTriggerData.ConvertDataVector3(ref val, ref value);

                InputsToTrigger.Add(val);
            }
        }

        public void OutputVector4(ref Socket socketOutput, Vector4 value)
        {
            NativeArray<Entity> edgesEntities = OutputsToEdgesEntity.GetValueArray(Allocator.TempJob);
            for (int i = 0; i < edgesEntities.Length; i++)
            {
                EdgeRuntime edge = EdgeRuntime[edgesEntities[i]];

                TriggerData val = new TriggerData() { Socket = edge.SocketInput };
                ConvertTriggerData.ConvertDataVector4(ref val, ref value);

                InputsToTrigger.Add(val);
            }
        }

        [BurstCompile]
        public void Execute()
        {
            // Step 1 : Process each nodes
            for (int i = 0; i < ProcessThisFrame.Length; i++)
            {
                Entity nodeEntity = ProcessThisFrame[i];
                
                GraphContext graphContext = new GraphContext();
                NodeRuntime nodeRuntime = NodeRuntime[nodeEntity];
                nodeRuntime.FunctionPointerUpdate.Invoke(ref nodeRuntime.NodeData, ref graphContext);
                NodeRuntime[nodeEntity] = nodeRuntime;
                
                ProcessGraphContext(ref nodeEntity, ref graphContext);
            }

            // Step 2 : Traverse edges. Always trigger signals last. Each time we process a signal, execute the inputs of other types.
            for (int indexSignal = 0; indexSignal < InputsToTriggerSignal.Length; indexSignal++)
            {
                for (int indexTrigger = 0; indexTrigger < InputsToTrigger.Length; indexTrigger++)
                {
                    TriggerData socketTrigger = InputsToTrigger[indexTrigger];
                    GraphContext graphContextTrigger = new GraphContext();
                    Entity nodeEntityTrigger = socketTrigger.Socket.NodeEntity;
                    NodeRuntime nodeRuntimeTrigger = NodeRuntime[nodeEntityTrigger];
                    nodeRuntimeTrigger.FunctionPointerInputTrigger.Invoke(ref nodeRuntimeTrigger.NodeData, ref socketTrigger, ref graphContextTrigger);
                    NodeRuntime[nodeEntityTrigger] = nodeRuntimeTrigger;
                    ProcessGraphContext(ref socketTrigger.Socket.NodeEntity, ref graphContextTrigger);
                }

                InputsToTrigger.Clear();
                
                TriggerData socket = InputsToTrigger[indexSignal];
                Entity nodeEntity = socket.Socket.NodeEntity;
                GraphContext graphContext = new GraphContext();
                NodeRuntime nodeRuntime = NodeRuntime[nodeEntity];
                nodeRuntime.FunctionPointerInputTrigger.Invoke(ref nodeRuntime.NodeData, ref socket, ref graphContext);
                NodeRuntime[nodeEntity] = nodeRuntime;
                ProcessGraphContext(ref nodeEntity, ref graphContext);
            }

            InputsToTriggerSignal.Clear();

            // Step 3 : Add / Remove updates after processing all the nodes
            for (int i = 0; i < ProcessToAdd.Length; i++)
            {
                Entity toAdd = ProcessToAdd[i];
                if (!ProcessThisFrame.Contains(toAdd))
                {
                    ProcessThisFrame.Add(toAdd);
                }
            }

            ProcessToAdd.Clear();

            for (int i = 0; i < ProcessToRemove.Length; i++)
            {
                int indexOfEntity = ProcessThisFrame.IndexOf(ProcessToRemove[i]);
                if (indexOfEntity >= 0)
                {
                    ProcessThisFrame.RemoveAtSwapBack(indexOfEntity);
                }
            }

            ProcessToRemove.Clear();
        }
    }

    List<VisualScriptingGraphJob> jobs = new List<VisualScriptingGraphJob>();
    protected override void OnStartRunning()
    {
        EntityQuery graphNodesQuery = GetEntityQuery(typeof(NodeRuntime), typeof(NodeSharedComponentData));
        EntityQuery graphEdgesQuery = GetEntityQuery(typeof(EdgeRuntime), typeof(NodeSharedComponentData));

        Entities.ForEach((Entity entity, ref VisualScriptingGraphTag vsGraph) =>
        {
            graphNodesQuery.SetSharedComponentFilter(new NodeSharedComponentData { Graph = entity });
            graphEdgesQuery.SetSharedComponentFilter(new NodeSharedComponentData { Graph = entity });

            VisualScriptingGraphJob job = new VisualScriptingGraphJob()
            {
                GraphEntity = entity,
                // Allocation currently leaking
                NodesInGraph = graphNodesQuery.ToComponentDataArray<NodeRuntime>(Allocator.Persistent),
                NodeEntities = graphNodesQuery.ToEntityArray(Allocator.Persistent),
                EdgesInGraph = graphEdgesQuery.ToComponentDataArray<EdgeRuntime>(Allocator.Persistent),
                EdgeEntities = graphEdgesQuery.ToEntityArray(Allocator.Persistent),
                NodeRuntime = GetComponentDataFromEntity<NodeRuntime>(),
                EdgeRuntime = GetComponentDataFromEntity<EdgeRuntime>(),
            };
            job.Initialize();
            jobs.Add(job);
        }).WithoutBurst().Run();
    }

    protected override void OnStopRunning()
    {
        for (int i = 0; i < jobs.Count; i++)
        {
            jobs[i].Dispose();
            jobs[i].NodesInGraph.Dispose();
            jobs[i].NodeEntities.Dispose();
            jobs[i].EdgesInGraph.Dispose();
            jobs[i].EdgeEntities.Dispose();
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        bool hasProcessed = false;

        JobHandle jobHandle = inputDeps;

        do
        {
            hasProcessed = false;

            for (int i = 0; i < jobs.Count; i++)
            {
                if (jobs[i].NeedProcessing())
                {
                    jobHandle = jobs[i].Schedule(jobHandle);
                    hasProcessed = true;
                }
            }
            jobHandle.Complete();

            for (int i = 0; i < jobs.Count; i++)
            {
                // Todo : Process external outputs.
                // Each inputs needs to be set in the job.
                // If there's an input set, hasProcessed = true;
            }
        }
        while (hasProcessed == true);

        return jobHandle;
    }
}