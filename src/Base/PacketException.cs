using System;

namespace iPanelHost.Base;

public class PacketException : Exception
{
    public PacketException(string? message)
        : base(message) { }
}
