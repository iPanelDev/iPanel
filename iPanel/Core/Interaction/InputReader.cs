using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace iPanel.Core.Interaction;

public class InputReader
{
    private readonly IReadOnlyDictionary<string, Command> _commandParser;

    private readonly string _allCommands;
    private static readonly char[] _separator = new char[] { '\x20' };
    private readonly IHost _host;
    private IServiceProvider Services => _host.Services;

    private ILogger<InputReader> Logger => Services.GetRequiredService<ILogger<InputReader>>();

    public InputReader(IHost host)
    {
        _host = host;

        var stringBuilder = new StringBuilder();
        var attributes = new List<(CommandDescriptionAttribute, CommandUsageAttribute[])>();
        var dict = new Dictionary<string, Command>();

        stringBuilder.AppendLine($"iPanel {Constant.Version}");
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            var attribute = type.GetCustomAttribute<CommandDescriptionAttribute>();
            if (attribute is null)
                continue;

            var handler = (Command?)Activator.CreateInstance(type, _host);

            if (handler is null)
                continue;

            dict[attribute.RootCommand] = handler;
            attributes.Add(
                (attribute, type.GetCustomAttributes<CommandUsageAttribute>().ToArray())
            );
        }

        attributes.Sort((a, b) => b.Item1.Priority - a.Item1.Priority);
        foreach (var attributePair in attributes)
        {
            stringBuilder.AppendLine(
                $"▪ {attributePair.Item1.RootCommand}  {attributePair.Item1.Description}"
            );

            foreach (var usage in attributePair.Item2)
                stringBuilder.AppendLine($"  ▪ {usage.Example}  {usage.Description}");
        }

        _allCommands = stringBuilder.ToString();
        _commandParser = dict;
    }

    public void Start(CancellationToken cancellationToken)
    {
        Task.Run(() => ReadLine(cancellationToken), cancellationToken);
    }

    private void ReadLine(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var input = Console.ReadLine();

            if (input is null)
                continue;

            if (Parse(input.Trim().TrimStart('/'), out var args))
                Handle(args);
            else
                Logger.LogError("语法错误：含有未闭合的冒号（\"）。若要作为参数的一部分传输，请使用\\\"进行转义");
        }
    }

    public static bool Parse(string line, [NotNullWhen(true)] out List<string>? result)
    {
        var args = new List<string>();

        if (!line.Contains('"') || !line.Contains(' '))
            args.AddRange(line.Split(_separator, options: StringSplitOptions.RemoveEmptyEntries));
        else
        {
            var inColon = false;
            var colonIndex = -1;

            var stringBuilder = new StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                switch (c)
                {
                    case '\x20':
                        if (!inColon)
                        {
                            args.Add(stringBuilder.ToString());
                            stringBuilder.Clear();
                        }
                        else
                            stringBuilder.Append(c);
                        break;

                    case '"':
                        if (inColon && colonIndex > 0)
                            colonIndex = -1;

                        if (i > 1 && line[i - 1] != '\\')
                        {
                            inColon = !inColon;
                            colonIndex = i;
                        }
                        else
                        {
                            stringBuilder.Remove(stringBuilder.Length - 1, 1);
                            stringBuilder.Append(c);
                        }

                        break;

                    default:
                        stringBuilder.Append(c);
                        break;
                }
            }

            if (inColon)
            {
                result = null;
                return false;
            }
            if (stringBuilder.Length != 0)
                args.Add(stringBuilder.ToString());
        }
        result = args;
        return true;
    }

    private void Handle(List<string> args)
    {
        if (args.Count == 0)
        {
            Logger.LogError("未知命令。请使用\"help\"查看所有命令");
            return;
        }

        if (args[0] == "help" || args[0] == "?")
            Logger.LogInformation("{}", _allCommands);
        else if (!_commandParser.TryGetValue(args[0], out var parser))
            Logger.LogError("未知命令。请使用\"help\"查看所有命令");
        else
            try
            {
                parser.Parse(args.ToArray());
            }
            catch (Exception e)
            {
                Logger.LogError(e, "解析命令时出现异常");
            }
    }
}
