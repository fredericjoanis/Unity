using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Prototype
{
    // All blittable values goes inside this struct. It needs to inherit from IComponentData
    // Needs to be declared Serializable for visibility in editor
    [System.Serializable]
    public struct RecommandationECSData : IComponentData
    {
        public int SomeIntValue;

        // All GameObjects from the ECS should be referenced as entities here.
        public Entity SomeEntity;
    }

    // Implement one of those 3 systems
    // 1- Hybrid ECS System which is a ComponentSystem where you can still access non-blittable data.
    // This is the easiest mode to work with and have a foot in the ECS world. I expect a lot of Gameplay code to fall here.
    // Use this if you need
    // a. To access Non-Blittable data defined in your class inheriting ECS<T>
    // b. Debugging or exploratory development — sometimes it is easier to observe what is going on when the code is running on the main thread.You can, for example, log debug text and draw debug graphics.
    // c. When the system needs to access or interface with other APIs that can only run on the main thread — this can help you gradually convert your game systems to ECS rather than having to rewrite everything from the start.
    // d. The amount of work the system performs is less than the small overhead of creating and scheduling a Job. (We recommand any system taking below 0.2ms to run on main thread).
    public class HybridECSSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // Entity entity is optional. Put it if you need it.
            Entities.ForEach((Entity entity, ref RecommandationECSData data, RecommandationECS recommandationECS) =>
            {
                data.SomeIntValue++;
                if (recommandationECS.SomeString == "abcdef")
                {
                    data.SomeIntValue *= 2;
                }
            });
        }
    }

    // 2- Main Thread ECS System : A component system where you only work with blittable data. You need this or Pure ECS if your entity is in a SubScene.
    // a. Debugging or exploratory development — sometimes it is easier to observe what is going on when the code is running on the main thread.You can, for example, log debug text and draw debug graphics.
    // b. When the system needs to access or interface with other APIs that can only run on the main thread — this can help you gradually convert your game systems to ECS rather than having to rewrite everything from the start.
    // c. The amount of work the system performs is less than the small overhead of creating and scheduling a Job. (We recommand any system taking below 0.2ms to run on main thread).
    public class MainThreadECSSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref RecommandationECSData data) =>
            {
                data.SomeIntValue++;
            });
        }
    }

    // 3- Pure ECS System : Pure means ECS + Job + Burst
    // Use it when you need maximum performance on CPU heavy systems. (Any system taking > 0.2ms)
    public class PureECSSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new PureECSJob();
            return job.Schedule(this, inputDeps);
        }

        // Use the BurstCompile as much as possible
        [BurstCompile]
        struct PureECSJob : IJobForEach<RecommandationECSData>
        {
            // The [ReadOnly] attribute tells the job scheduler that this job will not write to rotSpeedIJobForEach
            public void Execute(ref RecommandationECSData recommandationData)
            {
                recommandationData.SomeIntValue -= 1;
            }
        }
    }

    // Your classes need to inherit from
    // public class ECS<TData> : MonoBehaviour, IConvertGameObjectToEntity
    // where TData : struct, IComponentData
    public class RecommandationECS : ECS<RecommandationECSData>
    {
        // All non-blittable data goes here.
        public string SomeString;
        public GameObject SomeGameObject;

        // We need to manually convert all game objects referenced to their entity counter part here.
        // If you have no conversion to do, the EntityData will converted in the base class.
        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            EntityData.SomeEntity = conversionSystem.GetPrimaryEntity(SomeGameObject);
            base.Convert(entity, dstManager, conversionSystem);
        }

        /*
        Convert To Entity : Convert and Inject
        ¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯
            public virtual void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
            {
                // This line is done for you in the base ECS class.
                dstManager.AddComponentData(entity, EntityData);
                // With Convert and Inject, the object stays in the scene. The HybridECSSystem will recognize match the MonoBehavior.
            }

        Convert To Entity : Convert and Destroy
        ¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯
            public virtual void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
            {
                dstManager.AddComponentData(entity, EntityData);
                // With AddComponentObject, we'll be able to access the MonoBehavior data in the ComponentSystem.
                // If we omit it, we do not have access to the MonoBehavior data anymore.
                dstManager.AddComponentObject(entity, this);
            }

        Subscene
        ¯¯¯¯¯¯¯¯
            public virtual void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
            {
                dstManager.AddComponentData(entity, EntityData);
                // Subscenes do not support AddComponentObject. Subscenes is Pure ECS.
            }
        */
    }
}