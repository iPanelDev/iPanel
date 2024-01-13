using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using iPanel.Utils;

using Swan.Logging;

namespace iPanel;

public static class Program
{
    public static int Main(string[] args)
    {
        Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

        Console.OutputEncoding = EncodingsMap.UTF8;
        Console.InputEncoding = EncodingsMap.UTF8;
        Console.CancelKeyPress += (_, _) => Console.WriteLine("^C");

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            Console.Title = $"iPanel {Constant.Version}";

        Logger.UnregisterLogger<ConsoleLogger>();
        Logger.RegisterLogger<SimpleLogger>();

        TaskScheduler.UnobservedTaskException += (_, e) =>
            Logger.Error(e.Exception, nameof(TaskScheduler), string.Empty);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Logger.Error((e.ExceptionObject as Exception)!, nameof(AppDomain), string.Empty);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            SaveException(e.ExceptionObject as Exception);

        return CommandLineHelper.Create().Invoke(args);
    }

    private static void SaveException(Exception? e)
    {
        Directory.CreateDirectory("logs");

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"时间: {DateTime.Now}");
        stringBuilder.AppendLine($"版本: {Constant.Version}");

        var commandlineArgs = Environment.GetCommandLineArgs();
        if (commandlineArgs.Length > 0 && File.Exists(commandlineArgs[0]))
        {
            stringBuilder.AppendLine($"文件名: {Path.GetFileName(commandlineArgs[0])}");
            stringBuilder.AppendLine(
                $"MD5: {Encryption.GetMD5(File.ReadAllBytes(commandlineArgs[0]))}"
            );
        }
        stringBuilder.AppendLine(e?.ToString());

        var fileName = $"logs/crash-{DateTime.Now:yyyy-MM-dd-hh:mm:ss}.txt";
        File.WriteAllText(fileName, stringBuilder.ToString());
        Console.WriteLine(
            $"完整日志已保存在\"{fileName}\"，你可以在<https://github.com/iPanelDev/iPanel/issues/new>上反馈此问题"
        );

        if (!Console.IsInputRedirected)
            Console.ReadKey(true);
    }
}
