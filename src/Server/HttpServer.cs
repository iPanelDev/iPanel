using EmbedIO;
using EmbedIO.Security;
using iPanelHost.Utils;
using iPanelHost.WebSocket;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iPanelHost.Server
{
    internal static class HttpServer
    {
        private static WebServer? _server;

        /// <summary>
        /// 启动服务器
        /// </summary>
        public static void Start()
        {
            _server = new(CreateOptions());
            _server.WithModule(new WsModule("/ws"));

            if (Program.Setting.WebServer.AllowCrossOrigin)
            {
                _server.WithCors();
            }

            if (Directory.Exists(Program.Setting.WebServer.Directory))
            {
                _server.WithStaticFolder("/", Program.Setting.WebServer.Directory, Program.Setting.WebServer.DisableFilesHotUpdate);
                _server.HandleHttpException(Handle404);

                if (Program.Setting.WebServer.DisableFilesHotUpdate)
                {
                    FileSystemWatcher fileSystemWatcher = new(Program.Setting.WebServer.Directory)
                    {
                        EnableRaisingEvents = true,
                        IncludeSubdirectories = true,
                    };
                    fileSystemWatcher.Changed += (_, e) => Logger.Warn($"网页文件目录中“{e.Name}”貌似发生了更改:{e.ChangeType}。但是由于关闭了热更新文件，所以这些更改不会被应用");
                }
            }
            else
            {
                Logger.Warn("静态网页目录不存在");
            }

            _server.WithIPBanning((ipModule) => ipModule.WithMaxRequestsPerSecond(1));
            _server.RunAsync();

        }

        /// <summary>
        /// 关闭服务器
        /// </summary>
        public static void Stop()
        {
            _server?.Dispose();
        }

        public static void Restart()
        {
            Stop();
            _404HtmlContent = null;
            Start();
        }

        /// <summary>
        /// 新建服务器选项
        /// </summary>
        private static WebServerOptions CreateOptions()
        {
            WebServerOptions options = new();

            try
            {
                Program.Setting.WebServer.UrlPrefixes.ToList().ForEach((url) => options.AddUrlPrefix(url));
            }
            catch (Exception e)
            {
                Logger.Fatal(e.ToString());
            }

            if (Program.Setting.WebServer.Certificate.Enable)
            {
                options.AutoLoadCertificate = Program.Setting.WebServer.Certificate.AutoLoadCertificate;
                options.AutoRegisterCertificate = Program.Setting.WebServer.Certificate.AutoRegisterCertificate;
                if (string.IsNullOrEmpty(Program.Setting.WebServer.Certificate.Path))
                {
                    return options;
                }

                if (File.Exists(Program.Setting.WebServer.Certificate.Path))
                {
                    options.Certificate = string.IsNullOrEmpty(Program.Setting.WebServer.Certificate.Password) ?
                        new(Program.Setting.WebServer.Certificate.Path) :
                        new(Program.Setting.WebServer.Certificate.Path, Program.Setting.WebServer.Certificate.Password);
                }
                else
                {
                    Logger.Warn($"“{Program.Setting.WebServer.Certificate.Path}”不存在");
                }
            }

            return options;
        }

        /// <summary>
        /// 处理404
        /// </summary>
        private static async Task Handle404(IHttpContext context, IHttpException exception)
        {
            if (exception.StatusCode == 404)
            {
                if (Program.Setting.WebServer.DisableFilesHotUpdate)
                {
                    _404HtmlContent ??= File.Exists(_404HtmlPath) ? File.ReadAllText(_404HtmlPath) : null;
                }
                else
                {
                    _404HtmlContent = File.Exists(_404HtmlPath) ? File.ReadAllText(_404HtmlPath) : _404HtmlContent;
                }
                if (!string.IsNullOrEmpty(_404HtmlContent))
                {
                    context.Response.StatusCode = 200;
                    await context.SendStringAsync(_404HtmlContent!, "text/html", Encoding.UTF8);
                    return;
                }
            }
            context.Response.StatusCode = exception.StatusCode;
            await context.SendStandardHtmlAsync(exception.StatusCode);
        }

        private static string? _404HtmlPath => Path.Combine(Program.Setting.WebServer.Directory, Program.Setting.WebServer.Page404);

        private static string? _404HtmlContent;
    }
}
