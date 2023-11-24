using System.Collections.Generic;
using Microsoft.Extensions.Logging;

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

            logger = new SimpleLogger(categoryName);
            _loggers[categoryName] = logger;
            return logger;
        }
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}
