using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


namespace Prototype
{
    [System.Serializable]
    public struct AddComponentDataEntityData : IComponentData
    {
        public int Bli;
    }

    public class AddComponentDataEntitySystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, ref AddComponentDataEntityData testData) =>
            {
                testData.Bli++;
            });
        }
    }

    public class AddComponentDataSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((AddComponentData test, ref AddComponentDataEntityData testData) =>
            {
                testData.Bli++;
                if (test.otherString.Length > 10)
                {
                    test.otherString = "";
                }
                test.otherString += "b";
            });
        }
    }

    public class AddComponentData : MonoBehaviour, IConvertGameObjectToEntity
    {
        public AddComponentDataEntityData EntityData;

        public string otherString;
        public int Yyy;

        public virtual void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, EntityData);
            // The only difference with AddComponentObject is that we do not call dstManager.AddComponentObject(entity, this);
        }
    }
}