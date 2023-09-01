using System;

namespace iPanelHost.Base;

public class InternalException : Exception
{
    public InternalException(string message, Exception innerException) : base(message, innerException)
    { }

    public InternalException(string message) : base(message)
    { }
}
