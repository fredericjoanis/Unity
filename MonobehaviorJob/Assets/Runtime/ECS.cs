using Unity.Entities;
using UnityEngine;

public class ECS<TData> : MonoBehaviour, IConvertGameObjectToEntity
    where TData : struct, IComponentData
{
    [SerializeField] protected TData EntityData;

    public virtual void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, EntityData);
    }
}
