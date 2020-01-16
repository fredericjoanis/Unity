using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

public struct GraphContext
{
    public int _ProcessEachFrame;
    public int _StopProcessEachFrame;

    public int _TriggerOutput;
    public TriggerData TriggerData;

    public void ProcessEachFrame()
    {
        _ProcessEachFrame = 1;
    }

    public void StopProcessEachFrame()
    {
        _StopProcessEachFrame = 1;
    }

    public void OutputTrigger(ref Socket socket)
    {
        _TriggerOutput = 1;
        TriggerData.Socket = socket;
    }
    public void OutputTrigger(ref Socket socket, float value)
    {
        _TriggerOutput = 1;
        TriggerData.Socket = socket;
        if (TriggerData.Socket.SocketType != SocketType.Float)
        {
            // More Error handling here
            TriggerData.Socket.SocketType = SocketType.Float;
        }
        TriggerData.FloatValue = value;
    }
    public void OutputTrigger(ref Socket socket, int value)
    {
        _TriggerOutput = 1;
        TriggerData.Socket = socket;
        if (TriggerData.Socket.SocketType != SocketType.Int)
        {
            // More Error handling here
            TriggerData.Socket.SocketType = SocketType.Int;
        }
        TriggerData.IntValue = value;
    }
    public void OutputTrigger(ref Socket socket, Vector2 value)
    {
        _TriggerOutput = 1;
        TriggerData.Socket = socket;
        if (TriggerData.Socket.SocketType != SocketType.Vector2)
        {
            // More Error handling here
            TriggerData.Socket.SocketType = SocketType.Vector2;
        }
        TriggerData.Vector2 = value;
    }
    public void OutputTrigger(ref Socket socket, Vector3 value)
    {
        _TriggerOutput = 1;
        TriggerData.Socket = socket;
        if (TriggerData.Socket.SocketType != SocketType.Vector3)
        {
            // More Error handling here
            TriggerData.Socket.SocketType = SocketType.Vector3;
        }
        TriggerData.Vector3 = value;
    }
    public void OutputTrigger(ref Socket socket, Vector4 value)
    {
        _TriggerOutput = 1;
        TriggerData.Socket = socket;
        if (TriggerData.Socket.SocketType != SocketType.Vector4)
        {
            // More Error handling here
            TriggerData.Socket.SocketType = SocketType.Vector4;
        }
        TriggerData.Vector4 = value;
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



        // Auto-generated
        [NativeDisableParallelForRestrictionAttribute]
        public ComponentDataFromEntity<ConsoleLogComponentData> ConsoleLogComponentDatas;
        [NativeDisableParallelForRestrictionAttribute]
        public ComponentDataFromEntity<WaitComponentData> WaitComponentDatas;
        [NativeDisableParallelForRestrictionAttribute]
        public ComponentDataFromEntity<SetFloatComponentData> SetFloatComponentDatas;
        [NativeDisableParallelForRestrictionAttribute]
        public ComponentDataFromEntity<StartComponentData> StartComponentDatas;
        //


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
                NodeData nodeData = GetNodeData(nodeEntity, ref nodeRuntime);
                NodesInGraph[i].FunctionPointerInitialize.Invoke(ref nodeData, ref graphContext);
                SetNodeData(ref nodeEntity, ref nodeRuntime, ref nodeData);
                ProcessGraphContext(ref nodeEntity, ref graphContext);
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

        private NodeData GetNodeData(Entity entity, ref NodeRuntime nodeRuntime)
        {
            NodeTypeEnum nodeType = NodeTypeEnum.Undefined;
            nodeRuntime.FunctionPointerGetNodeType.Invoke(ref nodeType);

            // Auto-generated
            switch(nodeType)
            {
                case NodeTypeEnum.ConsoleLog:
                    return new NodeData() { ConsoleLogComponentData = ConsoleLogComponentDatas[entity] };
                case NodeTypeEnum.SetFloat:
                    return new NodeData() { SetFloatComponentData = SetFloatComponentDatas[entity] };
                case NodeTypeEnum.Start:
                    return new NodeData() { StartComponentData = StartComponentDatas[entity] };
                case NodeTypeEnum.Wait:
                    return new NodeData() { WaitComponentData = WaitComponentDatas[entity] };
            }

            return new NodeData();
        }

        private void SetNodeData(ref Entity entity, ref NodeRuntime nodeRuntime, ref NodeData nodeData)
        {
            NodeTypeEnum nodeType = NodeTypeEnum.Undefined;
            nodeRuntime.FunctionPointerGetNodeType.Invoke(ref nodeType);

            // Auto-generated
            switch (nodeType)
            {
                case NodeTypeEnum.ConsoleLog:
                    ConsoleLogComponentDatas[entity] = nodeData.ConsoleLogComponentData;
                    break;
                case NodeTypeEnum.SetFloat:
                    SetFloatComponentDatas[entity] = nodeData.SetFloatComponentData;
                    break;
                case NodeTypeEnum.Start:
                    StartComponentDatas[entity] = nodeData.StartComponentData;
                    break;
                case NodeTypeEnum.Wait:
                    WaitComponentDatas[entity] = nodeData.WaitComponentData;
                    break;
            }
        }

        // Need to find a way to have direct function calls inside this class 
        // or direct access to arrays in the FunctionPointer functions.
        // This graph context is a huge bottleneck.
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
                        OutputTrigger(ref graphContext.TriggerData.Socket);
                    break;
                    case SocketType.Int:
                        OutputTrigger(ref graphContext.TriggerData.Socket, graphContext.TriggerData.IntValue);
                        break;
                    case SocketType.Float:
                        OutputTrigger(ref graphContext.TriggerData.Socket, graphContext.TriggerData.FloatValue);
                        break;
                    case SocketType.Vector2:
                        OutputTrigger(ref graphContext.TriggerData.Socket, graphContext.TriggerData.Vector2);
                        break;
                    case SocketType.Vector3:
                        OutputTrigger(ref graphContext.TriggerData.Socket, graphContext.TriggerData.Vector3);
                        break;
                    case SocketType.Vector4:
                        OutputTrigger(ref graphContext.TriggerData.Socket, graphContext.TriggerData.Vector4);
                        break;
                }
            }
        }

        [BurstCompile]
        public void Execute()
        {
            // Step 1 : Process each nodes
            for (int i = 0; i < ProcessThisFrame.Length; i++)
            {
                Entity nodeEntity = ProcessThisFrame[i];
                var nodeRuntime = NodeRuntime[nodeEntity];

                GraphContext graphContext = new GraphContext();
                NodeData nodeData = GetNodeData(nodeEntity, ref nodeRuntime);
                NodeRuntime[nodeEntity].FunctionPointerUpdate.Invoke(ref nodeData, ref graphContext);
                // Burst is not allowing me to pass the job as the struct is non-blittable due to NativeArrays.
                // This adds a ton of code and a slowness. Need to find a better way
                //NodeRuntime[currentNode].FunctionPointerUpdate.Invoke(ref currentNode, ref this);
                SetNodeData(ref nodeEntity, ref nodeRuntime, ref nodeData);
                
                ProcessGraphContext(ref nodeEntity, ref graphContext);
            }

            // Step 2 : Traverse edges. Always trigger signals last. Each time we process a signal, execute the inputs of other types.
            for (int indexSignal = 0; indexSignal < InputsToTriggerSignal.Length; indexSignal++)
            {
                for (int indexTrigger = 0; indexTrigger < InputsToTrigger.Length; indexTrigger++)
                {
                    TriggerData socketTrigger = InputsToTrigger[indexTrigger];
                    GraphContext graphContextTrigger = new GraphContext();
                    NodeRuntime nodeRuntimeTrigger = NodeRuntime[socketTrigger.Socket.NodeEntity];
                    NodeData nodeDataTrigger = GetNodeData(socketTrigger.Socket.NodeEntity, ref nodeRuntimeTrigger);
                    nodeRuntimeTrigger.FunctionPointerInputTrigger.Invoke(ref nodeDataTrigger, ref socketTrigger, ref graphContextTrigger);
                    SetNodeData(ref socketTrigger.Socket.NodeEntity, ref nodeRuntimeTrigger, ref nodeDataTrigger);
                    ProcessGraphContext(ref socketTrigger.Socket.NodeEntity, ref graphContextTrigger);
                }

                InputsToTrigger.Clear();
                
                TriggerData socket = InputsToTrigger[indexSignal];
                Entity nodeEntity = socket.Socket.NodeEntity;
                GraphContext graphContext = new GraphContext();
                NodeRuntime nodeRuntime = NodeRuntime[nodeEntity];
                NodeData nodeData = GetNodeData(nodeEntity, ref nodeRuntime);
                nodeRuntime.FunctionPointerInputTrigger.Invoke(ref nodeData, ref socket, ref graphContext);
                SetNodeData(ref nodeEntity, ref nodeRuntime, ref nodeData);
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

    List<VisualScriptingGraphJob> jobs = new List<VisualScriptingGraphJob>();
    protected override void OnCreate()
    {
        EntityQuery graphNodesQuery = GetEntityQuery(typeof(NodeRuntime), typeof(NodeSharedComponentData));
        EntityQuery graphEdgesQuery = GetEntityQuery(typeof(EdgeRuntime), typeof(NodeSharedComponentData));

        Entities.ForEach((Entity entity, ref VisualScriptingGraphTag vsGraph) =>
        {
            graphNodesQuery.SetSharedComponentFilter(new NodeSharedComponentData { Graph = entity });

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


                /*
                Auto-Generated
                */
                ConsoleLogComponentDatas = GetComponentDataFromEntity<ConsoleLogComponentData>(),
                StartComponentDatas = GetComponentDataFromEntity<StartComponentData>(),
                SetFloatComponentDatas = GetComponentDataFromEntity<SetFloatComponentData>(),
                WaitComponentDatas = GetComponentDataFromEntity<WaitComponentData>(),
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