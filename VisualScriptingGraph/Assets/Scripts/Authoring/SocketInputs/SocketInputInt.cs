using Unity.Entities;
using UnityEngine;

public class SocketInputInt : SocketInput
{
    public int DefaultValue;

    public override SocketType GetSocketType()
    {
        return SocketType.Int;
    }
}