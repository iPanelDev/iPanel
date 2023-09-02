using iPanelHost.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace iPanelHost.Base;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class Setting
{
    public WebServerSetting WebServer { get; init; } = new();

    public Win32ConsoleSetting Win32Console { get; init; } = new();

    /// <summary>
    /// 调试模式
    /// </summary>
    public bool Debug { get; init; }

    /// <summary>
    /// 实例连接密码
    /// </summary>
    public string InstancePassword { get; init; } = string.Empty;

    /// <summary>
    /// 启动时显示更短的Logo
    /// </summary>
    public bool DisplayShorterLogoWhenStart { get; init; }

    [JsonProperty("_internalVersion")]
    public static int? InternalVersion => Constant.InternalVersion;

    /// <summary>
    /// 检查设置
    /// </summary>
    public void Check()
    {
        if (!(InternalVersion >= Constant.InternalVersion))
        {
            Logger.Warn("设置文件setting.json版本过低，建议删除后重新生成");
        }
        if (WebServer is null)
        {
            throw new SettingsException($"{nameof(WebServer)}数据异常");
        }
        if (Win32Console is null)
        {
            throw new SettingsException($"{nameof(Win32Console)}数据异常");
        }
        if (string.IsNullOrEmpty(InstancePassword))
        {
            throw new SettingsException($"{nameof(InstancePassword)}为空");
        }
        if (WebServer.UrlPrefixes is null)
        {
            throw new SettingsException($"{nameof(WebServer.UrlPrefixes)}为null");
        }
        if (WebServer.UrlPrefixes.Length == 0)
        {
            throw new SettingsException($"{nameof(WebServer.UrlPrefixes)}为空。你至少应该设置一个");
        }
        if (string.IsNullOrEmpty(WebServer.Directory))
        {
            throw new SettingsException($"{nameof(WebServer.Directory)}为空");
        }
        if (string.IsNullOrEmpty(WebServer.Page404))
        {
            throw new SettingsException($"{nameof(WebServer.Page404)}为空");
        }
        if (WebServer.MaxRequestsPerSecond <= 0)
        {
            throw new SettingsException($"{nameof(WebServer.MaxRequestsPerSecond)}超出范围");
        }
        if (WebServer.Certificate is null)
        {
            throw new SettingsException($"{nameof(WebServer.Certificate)}为null");
        }
        if (WebServer.Certificate.Enable && Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            Logger.Warn("网页证书可能在非Windows系统下不可用");
        }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class WebServerSetting
    {
        /// <summary>
        /// URL前缀
        /// </summary>
        public string[] UrlPrefixes { get; init; } = { "http://127.0.0.1:30001" };

        /// <summary>
        /// 本地网页文件夹
        /// </summary>
        public string Directory { get; init; } = "dist";

        /// <summary>
        /// 禁用热更新文件
        /// </summary>
        public bool DisableFilesHotUpdate { get; init; } = true;

        /// <summary>
        /// 404网页
        /// </summary>
        public string Page404 { get; init; } = "index.html";

        /// <summary>
        /// 允许跨源
        /// </summary>
        public bool AllowCrossOrigin { get; init; }

        /// <summary>
        /// 每秒最大请求数量
        /// </summary>
        public int MaxRequestsPerSecond { get; init; } = 30;

        /// <summary>
        /// 封禁时长（分钟）
        /// </summary>
        public int BanMinutes { get; init; } = 30;

        /// <summary>
        /// 白名单
        /// </summary>
        public string[] WhiteList { get; init; } = Array.Empty<string>();

        public CertificateSettings Certificate { get; init; } = new();
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Win32ConsoleSetting
    {
        public bool AllowWindowClosing { get; init; }
        public bool AllowQuickEditAndInsert { get; init; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class CertificateSettings
    {
        public bool Enable { get; init; }

        public bool AutoRegisterCertificate { get; init; }

        public bool AutoLoadCertificate { get; init; }

        public string? Path;

        public string? Password;
    }
}
