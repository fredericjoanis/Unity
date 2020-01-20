using UnityEngine;

public class SocketInputVector3 : SocketInput
{
    public Vector3 DefaultValue;

    public override SocketType GetSocketType()
    {
        return SocketType.Vector3;
    }
}
