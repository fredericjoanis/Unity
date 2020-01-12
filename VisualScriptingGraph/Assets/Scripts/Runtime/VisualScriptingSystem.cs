using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

/*
public struct Node : IComponentData
{
   // FunctionPointer<>
}

public struct NodeActivatedTag : IComponentData
{
}

[GenerateAuthoringComponent]
public struct OnStartData : IComponentData
{
    
}

[GenerateAuthoringComponent]
public struct OnStartInputs : IComponentData
{

}

[GenerateAuthoringComponent]
public struct WaitInputs : IComponentData
{
    public Edge trigger;
    public EdgeFloat time;
    float defaultTime;
}

[GenerateAuthoringComponent]
public struct SetFloatData : IComponentData
{
    public float floatValue;
}

[GenerateAuthoringComponent]
public struct SetFloatInputs : IComponentData
{
    public Edge trigger;
}


// Instruct Burst to look for static methods with [BurstCompile] attribute
[BurstCompile]
public class NodeImplementation
{
    [BurstCompile]
    public static SetFloatData SetFloatSetup(ref SetFloatData inputs)
    {
        return new SetFloatData() { floatValue = inputs.floatValue };
    }
    
    [BurstCompile]
    public static void WaitSetup(EntityManager entityManager, Entity entity)
    {
        var inputs = entityManager.GetComponentData<WaitInputs>(entity);
        entityManager.SetComponentData(entity, new WaitData() { waitStartTime = inputs.time.value });
    }

    public delegate void Setup<T>(ComponentDataFromEntity<T> entityManager, Entity entity);
}

public class VisualScriptingSystem : JobComponentSystem
{
    NativeMultiHashMap<Entity, Edge> Edges;


    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        FunctionPointer<NodeImplementation.Setup> functionPointerSetup = BurstCompiler.CompileFunctionPointer<NodeImplementation.Setup>(NodeImplementation.WaitSetup);

        var waitInputs = new WaitInputs();
        waitInputs.time.value = 1;

        var job = new VisualScriptingSystemJob()
        {
            inputs = waitInputs,
            setup = functionPointerSetup
        };

        return inputDeps;
    }

    // This declares a new kind of job, which is a unit of work to do.
    // The job is declared as an IJobForEach<Translation, Rotation>,
    // meaning it will process all entities in the world that have both
    // Translation and Rotation components. Change it to process the component
    // types you want.
    //
    // The job is also tagged with the BurstCompile attribute, which means
    // that the Burst compiler will optimize it for the best performance.
    [BurstCompile]
    struct VisualScriptingSystemJob : IJob
    {
        bool firstExecution;
        public FunctionPointer<NodeImplementation.Setup> setup;
        ComponentData

        public void Execute()
        {
            setup.Invoke(, ounodeComponentDatat );
        }
    }
}
*/
