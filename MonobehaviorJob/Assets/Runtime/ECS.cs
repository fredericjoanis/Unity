using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public class ECS<TData> : MonoBehaviour, IConvertGameObjectToEntity
    where TData : struct, IComponentData
{
    [SerializeField] protected TData EntityData;
    public Entity Entity { get; private set; }

    public virtual void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        Entity = entity;
        dstManager.AddComponentData(entity, EntityData);
    }
}
