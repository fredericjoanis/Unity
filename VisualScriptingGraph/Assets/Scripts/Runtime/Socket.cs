using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

public enum SocketType : System.UInt16
{
    Undefined,
    Signal,
    Int,
    Float,
    Vector2,
    Vector3,
    Vector4,
    Entity,
    BlobString,
}
public struct Socket : IComponentData
{
    public Entity SocketEntity;
    public Entity NodeEntity;
    public SocketType SocketType;
}


