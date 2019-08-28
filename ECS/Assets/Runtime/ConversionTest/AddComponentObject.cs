using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[System.Serializable]
public struct AddComponentObjectEntityData : IComponentData
{
    public int Bla;
}

public class AddComponentObjectEntitySystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref AddComponentObjectEntityData testData) =>
        {
            testData.Bla++;
        });
    }
}

public class AddComponentObjectSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((AddComponentObject test, ref AddComponentObjectEntityData testData) =>
        {
            testData.Bla++;
            if(test.someString.Length > 10)
            {
                test.someString = "";
            }
            test.someString += "a";
        });
    }
}

public class AddComponentObject : MonoBehaviour, IConvertGameObjectToEntity
{
    public AddComponentObjectEntityData EntityData;

    public string someString;
    public int Zzz;

    public virtual void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, EntityData);
        dstManager.AddComponentObject(entity, this);
    }
}