using System;

public interface IMessage
{
}

public struct Message
{
    public Guid componentGuid;
    public IntPtr message;
}

public enum MessageState
{
    Processed,
    SendToJob
}
