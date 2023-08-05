using System;

namespace iPanelHost.Base
{
    public class SettingsException : Exception
    {
        public SettingsException(string? message) : base(message) { }
        public SettingsException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}