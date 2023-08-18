using iPanelHost.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace iPanelHost.Base
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    internal class Setting
    {
        public WebServerSetting WebServer = new();

        public Win32ConsoleSetting Win32Console = new();

        public bool Debug;

        public string InstancePassword = string.Empty;

        public bool DisplayShorterLogoWhenStart;

        [JsonProperty("_internalVersion")]
        public int? InternalVersion = Constant.InternalVersion;

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
        internal class WebServerSetting
        {
            /// <summary>
            /// URL前缀
            /// </summary>
            public string[] UrlPrefixes = { "http://127.0.0.1:30001" };

            /// <summary>
            /// 本地网页文件夹
            /// </summary>
            public string Directory = "dist";

            /// <summary>
            /// 禁用热更新文件
            /// </summary>
            public bool DisableFilesHotUpdate = true;

            /// <summary>
            /// 404网页
            /// </summary>
            public string Page404 = "index.html";

            /// <summary>
            /// 允许跨源
            /// </summary>
            public bool AllowCrossOrigin;

            public CertificateSettings Certificate = new();
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        internal class Win32ConsoleSetting
        {
            public bool
                AllowWindowClosing,
                AllowQuickEditAndInsert;
        }


        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        internal class CertificateSettings
        {
            public bool
                Enable,
                AutoRegisterCertificate,
                AutoLoadCertificate;

            public string? Path;

            public string? Password;
        }
    }
}
