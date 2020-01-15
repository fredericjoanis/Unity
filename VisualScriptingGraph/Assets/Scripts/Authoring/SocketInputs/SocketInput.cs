using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public abstract class SocketInput : MonoBehaviour
{
    public Socket Socket;
    public abstract SocketType GetSocketType();

    public Socket ConvertToSocketRuntime(Entity nodeEntity, Entity socketEntity)
    {
        Socket = new Socket()
        {
            SocketType = GetSocketType(),
            SocketEntity = socketEntity,
            NodeEntity = nodeEntity
        };

        return Socket;
    }
}
