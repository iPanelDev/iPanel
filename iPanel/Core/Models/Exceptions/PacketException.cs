using System;

namespace iPanel.Core.Models.Exceptions;

public class PacketException : Exception
{
    public PacketException(string? message)
        : base(message) { }
}
