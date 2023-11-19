using iPanel.Core.Models.Settings;
using iPanel.Utils.Json;
using Swan.Logging;
using System;
using System.IO;
using System.Text.Json;

namespace iPanel.Core.Service;

public class SettingManager
{
    private readonly FileSystemWatcher _watcher = new("./");

    private DateTime _lastTime;

    public SettingManager()
    {
        _watcher.Changed += OnChanged;
    }

    public void Start() => _watcher.EnableRaisingEvents = true;

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if ((DateTime.Now - _lastTime).TotalSeconds < 0.1)
            return;

        _lastTime = DateTime.Now;
        if (Path.GetFileName(e.FullPath) == "setting.json")
            Logger.Warn("检测到设置文件已更改。若要应用此设置请输入\"reload\"命令重启服务器");
    }

    public static Setting ReadSetting() =>
        JsonSerializer.Deserialize<Setting>(
            File.ReadAllText("setting.json"),
            JsonSerializerOptionsFactory.CamelCase
        ) ?? throw new NullReferenceException("文件为空");
}
