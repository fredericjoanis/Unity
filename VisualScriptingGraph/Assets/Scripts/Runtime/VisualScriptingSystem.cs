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
    List<VisualScriptingExecution> jobs = new List<VisualScriptingExecution>();

    [BurstCompile]
    public struct VisualScriptingExecution : IJob
    {
        [NativeDisableParallelForRestrictionAttribute]
        public ComponentDataFromEntity<NodeRuntime> NodeRuntime;

        public NativeArray<Entity> ProcessThisFrame;
        private NativeArray<Entity> ToProcess;

        public void OutputTrigger(Entity outputTrigger)
        {
        }

        [BurstCompile]
        public void Execute()
        {
            ProcessThisFrame.CopyTo(ToProcess);

            for (int i = 0; i < ToProcess.Length; i++)
            {
                Entity currentNode = ToProcess[i];
                var nodeRuntime = NodeRuntime[currentNode];
                NodeRuntime[currentNode].FunctionPointerUpdate.Invoke(ref currentNode, ref nodeRuntime, ref this);
                NodeRuntime[currentNode] = nodeRuntime;
            }
        }
    }

    protected override void OnCreate()
    {
        Entities.ForEach((ref VisualScriptingGraphTag vsGraph) =>
        {
            VisualScriptingExecution job = new VisualScriptingExecution()
            {
                NodeRuntime = GetComponentDataFromEntity<NodeRuntime>(),
            };
        }).WithoutBurst().Run();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle jobHandle = inputDeps;
        for(int i = 0; i < jobs.Count; i++)
        {
            jobHandle = jobs[i].Schedule(jobHandle);
        }
        jobHandle.Complete();

        return jobHandle;
    }
}