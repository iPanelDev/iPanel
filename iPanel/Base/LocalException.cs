using System;

namespace iPanel.Base
{
    internal class LocalException : Exception
    {
        public LocalException() : base() { }
        public LocalException(string? message) : base(message) { }
        public LocalException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}