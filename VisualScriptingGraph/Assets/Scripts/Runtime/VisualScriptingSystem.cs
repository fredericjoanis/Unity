using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

[BurstCompile]
public class VisualScriptingSystem : JobComponentSystem
{
    [BurstCompile]
    public struct VisualScriptingExecution : IJob
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
                NodesInGraph[i].FunctionPointerInitialize.Invoke(ref nodeEntity, ref this);
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

        public void ProcessEachFrame(ref Entity entity)
        {
            ProcessToAdd.Add(entity);
        }

        public void StopProcessEachFrame(ref Entity entity)
        {
            ProcessToRemove.Add(entity);
        }

        [BurstCompile]
        public void Execute()
        {
            // Step 1 : Process each nodes
            for (int i = 0; i < ProcessThisFrame.Length; i++)
            {
                Entity currentNode = ProcessThisFrame[i];
                var nodeRuntime = NodeRuntime[currentNode];
                NodeRuntime[currentNode].FunctionPointerUpdate.Invoke(ref currentNode, ref this);
                NodeRuntime[currentNode] = nodeRuntime;
            }

            // Step 2 : Traverse edges. Always trigger signals last. Each time we process a signal, execute the inputs of other types.
            for (int indexSignal = 0; indexSignal < InputsToTriggerSignal.Length; indexSignal++)
            {
                for (int indexTrigger = 0; indexTrigger < InputsToTrigger.Length; indexTrigger++)
                {
                    TriggerData socketTrigger = InputsToTrigger[indexTrigger];
                    NodeRuntime[socketTrigger.Socket.NodeEntity].FunctionPointerInputTrigger.Invoke(ref socketTrigger, ref this);
                }

                InputsToTrigger.Clear();

                TriggerData socket = InputsToTrigger[indexSignal];
                NodeRuntime[socket.Socket.NodeEntity].FunctionPointerInputTrigger.Invoke(ref socket, ref this);
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

        #region OutputTriggers
        [BurstCompile]
        public void OutputTrigger(ref Socket socketOutput)
        {
            NativeArray<Entity> edgesEntities = OutputsToEdgesEntity.GetValueArray(Allocator.TempJob);
            for(int i = 0; i < edgesEntities.Length; i++)
            {
                EdgeRuntime edge = EdgeRuntime[edgesEntities[i]];
                
                TriggerData val = new TriggerData() { Socket = edge.SocketInput };
                InputsToTriggerSignal.Add(val);
            }
        }
        
        public void OutputTrigger(ref Socket socketOutput, float value)
        {
            NativeArray<Entity> edgesEntities = OutputsToEdgesEntity.GetValueArray(Allocator.TempJob);
            for (int i = 0; i < edgesEntities.Length; i++)
            {
                EdgeRuntime edge = EdgeRuntime[edgesEntities[i]];

                TriggerData val = new TriggerData() { Socket = edge.SocketInput };
                TriggerData.ConvertData(ref val, ref value);

                InputsToTrigger.Add(val);
            }
        }

        public void OutputTrigger(ref Socket socketOutput, int value)
        {
            NativeArray<Entity> edgesEntities = OutputsToEdgesEntity.GetValueArray(Allocator.TempJob);
            for (int i = 0; i < edgesEntities.Length; i++)
            {
                EdgeRuntime edge = EdgeRuntime[edgesEntities[i]];

                TriggerData val = new TriggerData() { Socket = edge.SocketInput };
                TriggerData.ConvertData(ref val, ref value);

                InputsToTrigger.Add(val);
            }
        }

        public void OutputTrigger(ref Socket socketOutput, Vector2 value)
        {
            NativeArray<Entity> edgesEntities = OutputsToEdgesEntity.GetValueArray(Allocator.TempJob);
            for (int i = 0; i < edgesEntities.Length; i++)
            {
                EdgeRuntime edge = EdgeRuntime[edgesEntities[i]];

                TriggerData val = new TriggerData() { Socket = edge.SocketInput };
                TriggerData.ConvertData(ref val, ref value);

                InputsToTrigger.Add(val);
            }
        }

        public void OutputTrigger(ref Socket socketOutput, Vector3 value)
        {
            NativeArray<Entity> edgesEntities = OutputsToEdgesEntity.GetValueArray(Allocator.TempJob);
            for (int i = 0; i < edgesEntities.Length; i++)
            {
                EdgeRuntime edge = EdgeRuntime[edgesEntities[i]];

                TriggerData val = new TriggerData() { Socket = edge.SocketInput };
                TriggerData.ConvertData(ref val, ref value);

                InputsToTrigger.Add(val);
            }
        }
        public void OutputTrigger(ref Socket socketOutput, Vector4 value)
        {
            NativeArray<Entity> edgesEntities = OutputsToEdgesEntity.GetValueArray(Allocator.TempJob);
            for (int i = 0; i < edgesEntities.Length; i++)
            {
                EdgeRuntime edge = EdgeRuntime[edgesEntities[i]];

                TriggerData val = new TriggerData() { Socket = edge.SocketInput };
                TriggerData.ConvertData(ref val, ref value);

                InputsToTrigger.Add(val);
            }
        }

        #endregion
    }

    List<VisualScriptingExecution> jobs = new List<VisualScriptingExecution>();
    protected override void OnCreate()
    {
        EntityQuery graphNodesQuery = GetEntityQuery(typeof(NodeRuntime), typeof(NodeSharedComponentData));
        EntityQuery graphEdgesQuery = GetEntityQuery(typeof(EdgeRuntime), typeof(NodeSharedComponentData));

        Entities.ForEach((Entity entity, ref VisualScriptingGraphTag vsGraph) =>
        {
            graphNodesQuery.SetSharedComponentFilter(new NodeSharedComponentData { Graph = entity });

            VisualScriptingExecution job = new VisualScriptingExecution()
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

    protected override void OnDestroy()
    {
        base.OnDestroy();

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