using System;

namespace iPanel.Core.Models.Exceptions;

public class SettingsException : Exception
{
    public SettingsException(string? message)
        : base(message) { }
}
