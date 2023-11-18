using iPanel.Core.Models.Exceptions;
using Swan.Logging;
using System;
using System.Text.Json.Serialization;

namespace iPanel.Core.Models.Settings;

public class Setting
{
    public bool Debug { get; init; }

    public string InstancePassword { get; init; } = string.Empty;

    [JsonPropertyName("_internalVersion")]
    public int InternalVersion { get; init; } = Constant.InternalVersion;

    public WebServerSetting WebServer { get; init; } = new();

    public void Check()
    {
        if (!(InternalVersion >= Constant.InternalVersion))
            Logger.Warn("设置文件setting.json版本过低，建议删除后重新生成");

        if (WebServer is null)
            Throw($"{nameof(WebServer)}数据异常");

        if (string.IsNullOrEmpty(InstancePassword))
            Throw($"{nameof(InstancePassword)}为空");

        if (WebServer!.UrlPrefixes is null)
            Throw($"{nameof(WebServer.UrlPrefixes)}为null");

        if (WebServer.UrlPrefixes!.Length == 0)
            Throw($"{nameof(WebServer.UrlPrefixes)}为空。你至少应该设置一个");

        if (string.IsNullOrEmpty(WebServer.Directory))
            Throw($"{nameof(WebServer.Directory)}为空");

        if (string.IsNullOrEmpty(WebServer.Page404))
            Throw($"{nameof(WebServer.Page404)}为空");

        if (WebServer.MaxRequestsPerSecond <= 0)
            Throw($"{nameof(WebServer.MaxRequestsPerSecond)}超出范围");

        if (WebServer.Certificate is null)
            Throw($"{nameof(WebServer.Certificate)}为null");

        if (WebServer.Certificate!.Enable && Environment.OSVersion.Platform != PlatformID.Win32NT)
            Logger.Warn("网页证书可能在非Windows系统下不可用");
    }

    private static void Throw(string message) => throw new SettingsException(message);
}
