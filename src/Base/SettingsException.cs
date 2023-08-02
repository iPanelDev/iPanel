using System;

namespace iPanelHost.Base
{
    internal class SettingsException : Exception
    {
        public SettingsException() : base() { }
        public SettingsException(string? message) : base(message) { }
        public SettingsException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}