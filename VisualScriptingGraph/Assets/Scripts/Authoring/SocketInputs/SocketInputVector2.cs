using UnityEngine;

public class SocketInputVector2 : SocketInput
{
    public Vector2 DefaultValue;

    public override SocketType GetSocketType()
    {
        return SocketType.Vector2;
    }
}
