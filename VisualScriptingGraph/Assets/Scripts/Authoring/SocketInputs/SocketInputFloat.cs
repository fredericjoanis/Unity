using Unity.Entities;
using UnityEngine;

public class SocketInputFloat : SocketInput
{
    public float DefaultValue;

    public override SocketType GetSocketType()
    {
        return SocketType.Float;
    }
}