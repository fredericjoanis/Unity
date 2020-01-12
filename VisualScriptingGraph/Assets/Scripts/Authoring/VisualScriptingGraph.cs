using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class VisualScriptingGraph : MonoBehaviour, IConvertGameObjectToEntity
{
    public List<Node> nodes;
    public List<Edge> edges;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //dstManager.AddComponentData(entity, new VisualScriptingGraphTag());
    }
}
