using Unity.Entities;
using UnityEngine;

public class SocketInputGameObject : SocketInput
{
    public GameObject DefaultValue;

    public override SocketType GetSocketType()
    {
        return SocketType.Entity;
    }
}
