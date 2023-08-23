using System;

namespace iPanelHost.Base
{
    public class SettingsException : Exception
    {
        public SettingsException(string? message) : base(message) { }
    }
}