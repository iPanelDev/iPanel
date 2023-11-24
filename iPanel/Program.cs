using iPanel.Utils;
using System;
using System.CommandLine;
using System.Threading.Tasks;
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

        Logger.UnregisterLogger<ConsoleLogger>();
        Logger.RegisterLogger<SimpleLogger>();

        TaskScheduler.UnobservedTaskException += (_, e) =>
            Logger.Error(e.Exception, nameof(TaskScheduler), string.Empty);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Logger.Error((e.ExceptionObject as Exception)!, nameof(AppDomain), string.Empty);

        return CommandLineHelper.Create().Invoke(args);
    }
}
