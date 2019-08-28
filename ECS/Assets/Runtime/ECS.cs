using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public abstract class ECS<TComponentData> : MonoBehaviour, IConvertGameObjectToEntity
    where TComponentData : struct, IComponentData
{
    [SerializeField] protected TComponentData ComponentData;
    public Entity Entity { get; private set; }

    public virtual void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        Entity = entity;
        dstManager.AddComponentData(entity, ComponentData);
    }
}
