using UnityEngine;

public class SocketInputVector4 : SocketInput
{
    public Vector4 DefaultValue;

    public override SocketType GetSocketType()
    {
        return SocketType.Vector4;
    }
}
