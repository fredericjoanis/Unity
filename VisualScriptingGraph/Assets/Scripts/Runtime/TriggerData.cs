
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

//[StructLayout(LayoutKind.Explicit)]
public struct TriggerData : IComponentData
{
    //[FieldOffset(0)]
    public Entity SocketInput;

    public Entity NodeInput;

    //[FieldOffset(18)]
    public int IntValue;

    //[FieldOffset(18)]
    public float FloatValue;

    //[FieldOffset(18)]
    public Vector2 Vector2;

    //[FieldOffset(18)]
    public Vector3 Vector3;

    //[FieldOffset(18)]
    public Vector4 Vector4;

    //[FieldOffset(18)]
    public Entity Entity;
}