using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    internal class Setting
    {
        public WSSetting WebSocket = new();
        public WebServerSetting WebServer = new();
        public OutputSetting Output = new();
        public bool Debug;

        public void Check()
        {
            if (string.IsNullOrEmpty(WebSocket.Addr))
            {
                throw new SettingsException($"{nameof(WebSocket.Addr)}为空");
            }
            if (string.IsNullOrEmpty(WebSocket.Password))
            {
                throw new SettingsException($"{nameof(WebSocket.Password)}为空");
            }
            if (WebServer.UrlPrefixes is null)
            {
                throw new SettingsException($"{nameof(WebServer.UrlPrefixes)}为null");
            }
            if (string.IsNullOrEmpty(WebServer.Directory))
            {
                throw new SettingsException($"{nameof(WebServer.Directory)}为空");
            }
            if (string.IsNullOrEmpty(WebServer.Page404))
            {
                throw new SettingsException($"{nameof(WebServer.Page404)}为空");
            }
            if (!Directory.Exists(WebServer.Directory))
            {
                throw new SettingsException($"{nameof(WebServer.Directory)}不存在", new DirectoryNotFoundException());
            }
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        internal struct WSSetting
        {
            public string Addr = "ws://0.0.0.0:30000";
            public string Password = string.Empty;

            public WSSetting() { }
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        internal struct WebServerSetting
        {
            public string[] UrlPrefixes = { "http://127.0.0.1:30001" };
            public string Directory = "dist";
            public bool DisableFilesHotUpdate = true;
            public string Page404 = "index.html";
            public WebServerSetting() { }
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        internal struct OutputSetting
        {
            public bool DisplayCallerMemberName;
        }
    }
}
