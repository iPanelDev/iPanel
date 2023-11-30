using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace iPanel.Utils;

public class SimpleLoggerProvider : ILoggerProvider
{
    private readonly Dictionary<string, ILogger> _loggers;

    public SimpleLoggerProvider()
    {
        _loggers = new();
    }

    public ILogger CreateLogger(string categoryName)
    {
        lock (_loggers)
        {
            if (_loggers.TryGetValue(categoryName, out ILogger? logger))
                return logger;

            logger = new SimpleLogger();
            _loggers[categoryName] = logger;
            return logger;
        }
    }

    public void Dispose()
    {
        _loggers.Clear();
        GC.SuppressFinalize(this);
    }
}
