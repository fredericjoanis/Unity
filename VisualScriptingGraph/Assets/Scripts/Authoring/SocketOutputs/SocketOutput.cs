using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public abstract class SocketOutput : MonoBehaviour
{
    public Socket Socket;

    public Socket ConvertToSocketRuntime(Entity nodeEntity, Entity socketEntity)
    {
        Socket = new Socket()
        {
            SocketType = SocketType.Undefined, // We don't care about output socket type
            SocketEntity = socketEntity,
            NodeEntity = nodeEntity
        };

        return Socket;
    }
}