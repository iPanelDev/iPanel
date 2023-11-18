using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Swan.Logging;

namespace iPanel.Core.Interaction;

#pragma warning disable CA1847

public class InputParser
{
    private readonly App _app;

    private readonly IReadOnlyDictionary<string, CommandParser> _commandParser;

    private readonly string _allCommands;

    private static readonly char[] _separator = new char[] { '\x20' };

    public InputParser(App app)
    {
        _app = app;

        var stringBuilder = new StringBuilder();
        var attributes = new List<CommandParserAttribute>();
        var dict = new Dictionary<string, CommandParser>();

        stringBuilder.AppendLine($"iPanel {Constant.Version}");
        foreach (var type in Assembly.GetCallingAssembly().GetTypes())
        {
            var attribute = type.GetCustomAttribute<CommandParserAttribute>();
            if (attribute is null)
                continue;

            var handler = (CommandParser?)Activator.CreateInstance(type, _app);

            if (handler is null)
                continue;

            dict[attribute.RootCommand] = handler;
            attributes.Add(attribute);
        }

        attributes.Sort((a, b) => b.Priority - a.Priority);
        foreach (var attribute in attributes)
        {
            if (!string.IsNullOrEmpty(attribute.Alias))
                stringBuilder.AppendLine(
                    $"- {attribute.RootCommand}/{attribute.Alias}  {attribute.Description}"
                );
            else
                stringBuilder.AppendLine($"- {attribute.RootCommand}  {attribute.Description}");
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
            if (string.IsNullOrEmpty(input))
                continue;

            Parse(input.Trim());
        }
    }

    private void Parse(string line)
    {
        var args = new List<string>();

        if (!line.Contains("\"") || !line.Contains("\x20"))
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
                Logger.Error("语法错误：含有未闭合的冒号（\"）。若要作为参数的一部分传输，请使用\\\"进行转义");
                return;
            }
            if (stringBuilder.Length != 0)
                args.Add(stringBuilder.ToString());
        }

        if (args.Count == 0)
            return;

        if (args[0] == "help" || args[0] == "?" || args[0] == "？")
            Logger.Info(_allCommands);
        else if (!_commandParser.TryGetValue(args[0], out var parser))
            Logger.Error("未知命令。请使用\"help\"查看所有命令");
        else
            try
            {
                parser.Parse(args.ToArray());
            }
            catch (Exception e)
            {
                Logger.Error(e, string.Empty, "解析命令时出现异常");
            }
    }
}
