using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public class SocketInputString : SocketInput
{
    public string DefaultValue;

    public override SocketType GetSocketType()
    {
        return SocketType.BlobString;
    }
}
