using System;

namespace iPanel.Core.Server.WebSocket.Handlers;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class HandlerAttribute : Attribute
{
    public HandlerAttribute(string path)
    {
        Path = path;
    }

    public string Path { get; }
}
