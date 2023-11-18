using iPanel.Core.Interaction;
using iPanel.Core.Models.Settings;
using iPanel.Core.Server;
using iPanel.Core.Service;
using iPanel.Utils;
using Swan.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace iPanel;

public class App : IDisposable
{
    public readonly CancellationTokenSource CancellationTokenSource;

    public HttpServer HttpServer { get; internal set; }

    public Setting Setting { get; internal set; }

    public UserManager UserManager { get; }

    private readonly SettingManager _settingManager;

    private readonly InputParser _inputParser;

    public App(Setting setting)
    {
        Logger.Info(Constant.Logo);
        Setting = setting;
        CancellationTokenSource = new();
        HttpServer = new(this);
        UserManager = new();
        _settingManager = new();
        _inputParser = new(this);

        LocalLogger.StaticLogLevel = Setting.Debug ? LogLevel.Debug : LogLevel.Info;
        Console.CancelKeyPress += (_, _) => Dispose();
    }

    public async Task StartAsync()
    {
        UserManager.Read();
        _settingManager.Start();
        HttpServer.StartAsync(CancellationTokenSource.Token);

        Logger.Info("启动完毕");
        _inputParser.Start(CancellationTokenSource.Token);
        while (!CancellationTokenSource.IsCancellationRequested)
        {
            await Task.Delay(0x114514);
        }
    }

    public void Reload()
    {
        HttpServer.Dispose();
        try
        {
            SettingManager.ReadSetting();
        }
        catch (Exception e)
        {
            Logger.Warn(e, string.Empty, "加载设置出现异常");
        }
        HttpServer = new(this);
        HttpServer.StartAsync(CancellationTokenSource.Token);
    }

    public void Dispose()
    {
        HttpServer.Dispose();
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
        Logger.Info("Goodbye.");
        GC.SuppressFinalize(this);
        Environment.Exit(0);
    }
}
