
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

//[StructLayout(LayoutKind.Explicit)]
public struct TriggerData : IComponentData
{
    //[FieldOffset(0)]
    public Entity SocketInput;

    //[FieldOffset(8)]
    public int IntValue;

    //[FieldOffset(8)]
    public float FloatValue;

    //[FieldOffset(8)]
    public Vector2 Vector2;

    //[FieldOffset(8)]
    public Vector3 Vector3;

    //[FieldOffset(8)]
    public Vector4 Vector4;

    //[FieldOffset(8)]
    public Entity Entity;
}