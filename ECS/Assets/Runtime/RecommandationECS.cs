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
    public struct RecommandationComponentData : IComponentData
    {
        public int SomeIntValue;

        // All GameObjects from the ECS should be referenced as entities here.
        public Entity SomeEntity;
    }

    // Your classes need to inherit from
    // public class ECS<TData> : MonoBehaviour, IConvertGameObjectToEntity
    // where TData : struct, IComponentData
    public class RecommandationECS : ECS<RecommandationComponentData>
    {
        // All non-blittable data goes here.
        public string SomeString;
        public GameObject SomeGameObject;

        /*
         * Do not implement Update(), FixedUpdate(), LateUpdate(). They are replaced by System.
         */

        // We need to manually convert all game objects referenced to their entity counter part here.
        // If you have no conversion to do, you do not need to override this function.
        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            ComponentData.SomeEntity = conversionSystem.GetPrimaryEntity(SomeGameObject);
            base.Convert(entity, dstManager, conversionSystem);

            /*
             * Usage of Convert To Entity script on a game object the Convert function.
             * 
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
                    // This line is done for you in the base ECS class.
                    dstManager.AddComponentData(entity, EntityData);
                    // With AddComponentObject, we'll be able to access the MonoBehavior data in the ComponentSystem.
                    // If we omit it, we do not have access to the MonoBehavior data anymore.
                    dstManager.AddComponentObject(entity, this);
                }

            Subscene : Effect of having a game object inside a subscene
            ¯¯¯¯¯¯¯¯
                public virtual void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
                {
                    // This line is done for you in the base ECS class.
                    dstManager.AddComponentData(entity, EntityData);
                    // Subscenes do not support AddComponentObject. You will receive an error.
                    // Game Objects do not exist inside a subscene.
                    // Entities in a subscenes only work with Main Thread ECS or Pure ECS.
                }
            */
        }
    }

    // Implement one of those 3 systems
    // 1- Hybrid ECS 
    // This is the easiest way to do an ECS and have a foot in the DOTS world.
    // Use this for
    // a. Access to Non-Blittable data defined in your class inheriting ECS<T>
    // b. When the system needs to access or interface with other APIs that can only run on the main thread.
    // c. The amount of work the system performs is less than the small overhead of creating and scheduling a Job. (We recommand any system taking below 0.2ms to run on main thread).
    public class HybridECS : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // Entity entity is optional. Put it if you need it.
            Entities.ForEach((Entity entity, ref RecommandationComponentData data, RecommandationECS recommandationECS) =>
            {
                data.SomeIntValue++;
                if (recommandationECS.SomeString == "abcdef")
                {
                    data.SomeIntValue *= 2;
                }
            });
        }
    }

    // 2- Main Thread ECS
    // Main Thread ECS is a ComponentSystem where you only work with blittable data.
    // If your entity is inside a SubScene, you need Main Thread ECS or Pure ECS to access the component data.
    // Having all your data blittable is one of the hardest part of ECS.
    // Once all your data is blittable, going to Pure ECS (ECS + Job system + Burst) becomes straightforward.
    // Use Main Thread ECS When the system needs to access or interface with other APIs that can only run on the main thread.
    // Use Main Thread ECS When The amount of work the system performs is less than the small overhead of creating and scheduling a Job. (We recommand any system taking below 0.2ms to run on main thread).
    // Prefer Main Thread ECS over Hybrid ECS.
    public class MainThreadECS : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref RecommandationComponentData data) =>
            {
                data.SomeIntValue++;
            });
        }
    }

    // 3- Pure ECS : Pure means ECS + Job system + Burst
    // Use it when you need maximum performance on CPU heavy systems. (Any system taking > 0.2ms)
    public class PureECS : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new PureECSJob();
            return job.Schedule(this, inputDeps);
        }

        // Use the BurstCompile as much as possible
        [BurstCompile]
        struct PureECSJob : IJobForEach<RecommandationComponentData>
        {
            // The [ReadOnly] attribute tells the job scheduler that this job will not write to rotSpeedIJobForEach
            public void Execute(ref RecommandationComponentData recommandationData)
            {
                recommandationData.SomeIntValue -= 1;
            }
        }
    }
}