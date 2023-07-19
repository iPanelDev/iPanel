using System;

namespace iPanel.Base
{
    internal class PacketException : Exception
    {
        public PacketException() : base() { }
        public PacketException(string? message) : base(message) { }
        public PacketException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}