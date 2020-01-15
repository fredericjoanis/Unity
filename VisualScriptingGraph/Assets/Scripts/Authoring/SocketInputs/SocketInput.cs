using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public abstract class SocketInput : MonoBehaviour
{
    public Entity Entity;
    public Entity NodeEntity;

    public abstract SocketType GetSocketType();
}
