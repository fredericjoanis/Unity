using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

//[BurstCompile]
public class VisualScriptingSystem : JobComponentSystem
{
    //[BurstCompile]
    public struct VisualScriptingGraphJob : IJob
    {
        public Entity GraphEntity;

        [ReadOnly] public NativeArray<NodeType> NodesInGraph;
        [ReadOnly] public NativeArray<EdgeRuntime> EdgesInGraph;
        [ReadOnly] public NativeArray<Entity> NodeEntities;
        [ReadOnly] public NativeArray<Entity> EdgeEntities;
        [ReadOnly] public ComponentDataFromEntity<NodeType> NodeType;
        [ReadOnly] public NativeMultiHashMap<Entity, Entity> OutputsToEdgesEntity;
        [ReadOnly] public ComponentDataFromEntity<EdgeRuntime> EdgeRuntime;
        [ReadOnly] public ComponentDataFromEntity<Socket> Socket;

        private NativeList<Entity> ProcessThisFrame;
        private NativeList<Entity> ProcessToAdd;
        private NativeList<Entity> ProcessToRemove;

        private NativeList<TriggerData> InputsToTriggerSignal;
        private NativeList<TriggerData> InputsToTrigger;

        public bool FirstProcessThisFrame;

        // The entity key is the External Graph
        //public NativeMultiHashMap<Entity, TriggerData> ExternalInputs;

        // Code-gen start
        public StartJob StartJob;
        public WaitJob WaitJob;
        // Code-gen end

        public void OnStartRunning()
        {
            ProcessThisFrame = new NativeList<Entity>(Allocator.Persistent);
            ProcessToAdd = new NativeList<Entity>(Allocator.Persistent);
            ProcessToRemove = new NativeList<Entity>(Allocator.Persistent);
            InputsToTriggerSignal = new NativeList<TriggerData>(Allocator.Persistent);
            InputsToTrigger = new NativeList<TriggerData>(Allocator.Persistent);
            //ExternalInputs = new NativeMultiHashMap<Entity, TriggerData>();
            OutputsToEdgesEntity = new NativeMultiHashMap<Entity, Entity>(EdgesInGraph.Length, Allocator.Persistent);

            for (int i = 0; i < NodesInGraph.Length; i++)
            {
                Entity node = NodeEntities[i];
                NodeTypeEnum nodeType = NodeType[node].Value;
                
                // Code-gen start.
                switch (nodeType)
                {
                    case NodeTypeEnum.Start:
                        StartJob.OnStartRunning(node, ref this);
                    break;
                    case NodeTypeEnum.Wait:
                        WaitJob.OnStartRunning(node, ref this);
                    break;
                }
                // Code-gen stop
            }

            if(ProcessToAdd.Length > 0)
            {
                ProcessThisFrame.Resize(ProcessToAdd.Length, NativeArrayOptions.UninitializedMemory);
                ProcessThisFrame.CopyFrom(ProcessToAdd.ToArray());
                ProcessToAdd.Clear();
            }

            for (int i = 0; i < EdgesInGraph.Length; i++)
            {
                OutputsToEdgesEntity.Add(EdgesInGraph[i].SocketOutput, EdgesInGraph[i].SocketInput);
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
            NodesInGraph.Dispose();
            NodeEntities.Dispose();
            EdgesInGraph.Dispose();
            EdgeEntities.Dispose();
        }

        public void FirstProcessInFrame()
        {
            FirstProcessThisFrame = true;
        }

        public bool NeedProcessing()
        {
            if (FirstProcessThisFrame)
            {
                return ProcessThisFrame.Length > 0 || InputsToTrigger.Length > 0 || InputsToTriggerSignal.Length > 0;
            }
            else
            {
                return InputsToTrigger.Length > 0 || InputsToTriggerSignal.Length > 0;
            }
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
        public void OutputSignal(Entity socket)
        {
            var inputSocket = OutputsToEdgesEntity.GetValuesForKey(socket);
            while(inputSocket.MoveNext())
            {
                TriggerData val = new TriggerData()
                {
                    SocketInput = inputSocket.Current,
                };
                InputsToTriggerSignal.Add(val);
            }
        }
        
        public void OutputFloat(Entity socket, float value)
        {
            var inputSocket = OutputsToEdgesEntity.GetValuesForKey(socket);
            while (inputSocket.MoveNext())
            {
                TriggerData triggerData = new TriggerData()
                {
                    SocketInput = inputSocket.Current,
                };

                switch (Socket[inputSocket.Current].SocketType)
                {
                    case SocketType.Float:
                        triggerData.FloatValue = value;
                        break;
                    case SocketType.Int:
                        triggerData.IntValue = (int)value;
                        break;
                    case SocketType.Vector2:
                        triggerData.Vector2 = new Vector2(value, 0);
                        break;
                    case SocketType.Vector3:
                        triggerData.Vector3 = new Vector3(value, 0);
                        break;
                    case SocketType.Vector4:
                        triggerData.Vector4 = new Vector4(value, 0);
                        break;
                    default:
                        // Error handling.
                        break;
                }

                InputsToTrigger.Add(triggerData);
            }
        }

        /*
        public void OutputInt(Entity socket, int value)
        {
            var edgesResult = OutputsToEdgesEntity.GetValuesForKey(socket);
            while (edgesResult.MoveNext())
            {
                EdgeRuntime edge = EdgeRuntime[edgesResult.Current];

                TriggerData triggerData = new TriggerData()
                {
                    SocketInput = edge.SocketInput,
                };

                switch (Socket[edge.SocketInput].SocketType)
                {
                    case SocketType.Float:
                        triggerData.FloatValue = value;
                        break;
                    case SocketType.Int:
                        triggerData.IntValue = value;
                        break;
                    case SocketType.Vector2:
                        triggerData.Vector2 = new Vector2(value, 0);
                        break;
                    case SocketType.Vector3:
                        triggerData.Vector3 = new Vector3(value, 0);
                        break;
                    case SocketType.Vector4:
                        triggerData.Vector4 = new Vector4(value, 0);
                        break;
                }

                InputsToTrigger.Add(triggerData);
            }
        }

        public void OutputVector2(Entity socket, Vector2 value)
        {
            var edgesResult = OutputsToEdgesEntity.GetValuesForKey(socket);
            while (edgesResult.MoveNext())
            {
                EdgeRuntime edge = EdgeRuntime[edgesResult.Current];

                TriggerData triggerData = new TriggerData()
                {
                    SocketInput = edge.SocketInput,
                };

                switch (Socket[edge.SocketInput].SocketType)
                {
                    case SocketType.Float:
                        triggerData.FloatValue = value.x;
                        break;
                    case SocketType.Int:
                        triggerData.IntValue = (int)value.x;
                        break;
                    case SocketType.Vector2:
                        triggerData.Vector2 = value;
                        break;
                    case SocketType.Vector3:
                        triggerData.Vector3 = new Vector3(value.x, value.y, 0);
                        break;
                    case SocketType.Vector4:
                        triggerData.Vector4 = new Vector4(value.x, value.y, 0, 0);
                        break;
                }

                InputsToTrigger.Add(triggerData);
            }
        }

        public void OuputVector3(Entity socket, Vector3 value)
        {
            var edgesResult = OutputsToEdgesEntity.GetValuesForKey(socket);
            while (edgesResult.MoveNext())
            {
                EdgeRuntime edge = EdgeRuntime[edgesResult.Current];

                TriggerData triggerData = new TriggerData()
                {
                    SocketInput = edge.SocketInput,
                };

                switch (Socket[edge.SocketInput].SocketType)
                {
                    case SocketType.Float:
                        triggerData.FloatValue = value.x;
                        break;
                    case SocketType.Int:
                        triggerData.IntValue = (int)value.x;
                        break;
                    case SocketType.Vector2:
                        triggerData.Vector2 = new Vector2(value.x, value.y);
                        break;
                    case SocketType.Vector3:
                        triggerData.Vector3 = value;
                        break;
                    case SocketType.Vector4:
                        triggerData.Vector4 = new Vector4(value.x, value.y, 0, 0);
                        break;
                }

                InputsToTrigger.Add(triggerData);
            }
        }

        public void OutputVector4(Entity socket, Vector4 value)
        {
            var edgesResult = OutputsToEdgesEntity.GetValuesForKey(socket);
            while (edgesResult.MoveNext())
            {
                EdgeRuntime edge = EdgeRuntime[edgesResult.Current];

                TriggerData triggerData = new TriggerData()
                {
                    SocketInput = edge.SocketInput,
                };

                switch (Socket[edge.SocketInput].SocketType)
                {
                    case SocketType.Float:
                        triggerData.FloatValue = value.x;
                        break;
                    case SocketType.Int:
                        triggerData.IntValue = (int)value.x;
                        break;
                    case SocketType.Vector2:
                        triggerData.Vector2 = new Vector2(value.x, value.y);
                        break;
                    case SocketType.Vector3:
                        triggerData.Vector3 = new Vector3(value.x, value.y, value.z);
                        break;
                    case SocketType.Vector4:
                        triggerData.Vector4 = value;
                        break;
                }

                InputsToTrigger.Add(triggerData);
            }
        }

    */
        

        [BurstCompile]
        public void Execute()
        {
            // Step 1 : Process each nodes
            if (FirstProcessThisFrame)
            {
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
                            WaitJob.Execute(node, ref this);
                            break;
                    }
                    // Code-gen stop
                }
                FirstProcessThisFrame = false;
            }


            // Step 2 : Traverse edges. Always trigger signals last. Each time we process a signal, execute the inputs of other types.
            for (int indexSignal = 0; indexSignal < InputsToTriggerSignal.Length; indexSignal++)
            {
                for (int indexTrigger = 0; indexTrigger < InputsToTrigger.Length; indexTrigger++)
                {
                    TriggerData triggerData2 = InputsToTrigger[indexTrigger];
                    Entity nodeTrigger = Socket[triggerData2.SocketInput].NodeEntity;
                    if (nodeTrigger != Entity.Null)
                    {
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
                }

                InputsToTrigger.Clear();
                
                TriggerData triggerData = InputsToTriggerSignal[indexSignal];
                Entity node = Socket[triggerData.SocketInput].NodeEntity;
                if (node != Entity.Null)
                {
                    NodeTypeEnum nodeType = NodeType[node].Value;

                    // Code-gen start. Assuming Burst is doing a Jump table.
                    switch (nodeType)
                    {
                        case NodeTypeEnum.Start:
                            StartJob.InputTriggered(node, ref triggerData, ref this);
                            break;
                        case NodeTypeEnum.Wait:
                            WaitJob.InputTriggered(node, ref triggerData, ref this);
                            break;
                    }
                    // Code-gen stop
                }
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

    private class GraphJobRef
    {
        public VisualScriptingGraphJob graphJob;
        public JobHandle jobHandle;
    }

    List<GraphJobRef> GraphJob = new List<GraphJobRef>();
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
                Socket = GetComponentDataFromEntity<Socket>(),
                NodeType = GetComponentDataFromEntity<NodeType>(),
                EdgeRuntime = GetComponentDataFromEntity<EdgeRuntime>(),
                StartJob = StartSystem.StartJob,
                WaitJob = WaitSystem.WaitJob,
            };
            job.OnStartRunning();
            GraphJob.Add(new GraphJobRef() { graphJob = job });
        }).WithoutBurst().Run();
    }

    protected override void OnStopRunning()
    {
        for (int i = 0; i < GraphJob.Count; i++)
        {
            GraphJob[i].graphJob.Dispose();
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        bool hasProcessed = false;
        bool firstIteration = true;

        //do
        {
            hasProcessed = false;

            for (int i = 0; i < GraphJob.Count; i++)
            {
                if(firstIteration)
                {
                    GraphJob[i].graphJob.FirstProcessInFrame();
                }

                if (GraphJob[i].graphJob.NeedProcessing())
                {
                    GraphJob[i].jobHandle = GraphJob[i].graphJob.Schedule();
                    
                    hasProcessed = true;
                }
            }

            // Todo: Transform this in JobHandle.CompleteAll();
            for (int i = 0; i < GraphJob.Count; i++)
            {
                GraphJob[i].jobHandle.Complete();
            }

            firstIteration = false;

            for (int i = 0; i < GraphJob.Count; i++)
            {
                // Todo : Process external outputs.
                // Each inputs needs to be set in the job.
                // If there's an input set, hasProcessed = true;
            }
        }
        //while (hasProcessed == true);

        return inputDeps;
    }
}