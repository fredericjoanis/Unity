using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public class VisualScriptingSystem : JobComponentSystem
{
    [BurstCompile]
    public struct VisualScriptingGraphJob : IJob
    {
        public Entity GraphEntity;

        [ReadOnly] public NativeArray<NodeType> NodesInGraph;
        [ReadOnly] public NativeArray<EdgeRuntime> EdgesInGraph;
        [ReadOnly] public NativeArray<Entity> NodeEntities;
        [ReadOnly] public NativeArray<Entity> EdgeEntities;

        [NativeDisableParallelForRestrictionAttribute]
        public ComponentDataFromEntity<NodeType> NodeType;

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

        // Code-gen start
        public StartJob StartJob;
        public WaitJob WaitJob;
        // Code-gen end

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
                Entity node = NodeEntities[i];
                NodeTypeEnum nodeType = NodeType[node].Value;
                
                // Code-gen start.
                switch (nodeType)
                {
                    case NodeTypeEnum.Start:
                        StartJob.Initialize(node, ref this);
                    break;
                    case NodeTypeEnum.Wait:
                        StartJob.Initialize(node, ref this);
                    break;
                }
                // Code-gen stop
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

        public void ProcessEachFrame(Entity node)
        {
            ProcessToAdd.Add(node);
        }

        public void StopProcessEachFrame(Entity node)
        {
            ProcessToRemove.Add(node);
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
                Entity node = ProcessThisFrame[i];
                NodeTypeEnum nodeType = NodeType[node].Value;

                // Code-gen start. Assuming Burst is doing a Jump table.
                switch (nodeType)
                {
                    case NodeTypeEnum.Start:
                        StartJob.Execute(node, ref this);
                    break;
                    case NodeTypeEnum.Wait:
                        WaitJob.Initialize(node, ref this);
                    break;
                }
                // Code-gen stop
            }

            // Step 2 : Traverse edges. Always trigger signals last. Each time we process a signal, execute the inputs of other types.
            for (int indexSignal = 0; indexSignal < InputsToTriggerSignal.Length; indexSignal++)
            {
                for (int indexTrigger = 0; indexTrigger < InputsToTrigger.Length; indexTrigger++)
                {
                    TriggerData triggerData2 = InputsToTrigger[indexTrigger];
                    Entity nodeTrigger = triggerData2.Socket.NodeEntity;
                    NodeTypeEnum nodeTypeTrigger = NodeType[nodeTrigger].Value;

                    // Code-gen start. Assuming Burst is doing a Jump table.
                    switch (nodeTypeTrigger)
                    {
                        case NodeTypeEnum.Start:
                            StartJob.InputTriggered(nodeTrigger, ref triggerData2, ref this);
                        break;
                        case NodeTypeEnum.Wait:
                            WaitJob.InputTriggered(nodeTrigger, ref triggerData2, ref this);
                        break;
                    }
                    // Code-gen stop
                }

                InputsToTrigger.Clear();
                
                TriggerData socket = InputsToTrigger[indexSignal];
                Entity node = socket.Socket.NodeEntity;
                NodeTypeEnum nodeType = NodeType[node].Value;

                // Code-gen start. Assuming Burst is doing a Jump table.
                switch (nodeType)
                {
                    case NodeTypeEnum.Start:
                        StartJob.InputTriggered(node, ref socket, ref this);
                        break;
                    case NodeTypeEnum.Wait:
                        WaitJob.InputTriggered(node, ref socket, ref this);
                    break;
                }
                // Code-gen stop
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
        EntityQuery graphNodesQuery = GetEntityQuery(typeof(NodeType), typeof(NodeSharedComponentData));
        EntityQuery graphEdgesQuery = GetEntityQuery(typeof(EdgeRuntime), typeof(NodeSharedComponentData));

        Entities.ForEach((Entity entity, ref VisualScriptingGraphTag vsGraph) =>
        {
            graphNodesQuery.SetSharedComponentFilter(new NodeSharedComponentData { Graph = entity });
            graphEdgesQuery.SetSharedComponentFilter(new NodeSharedComponentData { Graph = entity });

            VisualScriptingGraphJob job = new VisualScriptingGraphJob()
            {
                GraphEntity = entity,
                // Allocation currently leaking
                NodesInGraph = graphNodesQuery.ToComponentDataArray<NodeType>(Allocator.Persistent),
                NodeEntities = graphNodesQuery.ToEntityArray(Allocator.Persistent),
                EdgesInGraph = graphEdgesQuery.ToComponentDataArray<EdgeRuntime>(Allocator.Persistent),
                EdgeEntities = graphEdgesQuery.ToEntityArray(Allocator.Persistent),
                NodeType = GetComponentDataFromEntity<NodeType>(),
                EdgeRuntime = GetComponentDataFromEntity<EdgeRuntime>(),
                StartJob = StartSystem.StartJob,
                WaitJob = WaitSystem.WaitJob,
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