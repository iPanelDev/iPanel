using EmbedIO;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Swan.Logging;

namespace iPanel.HttpServer
{
    internal static class Server
    {
        private static WebServer? _server;

        private static Thread? _thread;

        /// <summary>
        /// 启动服务器
        /// </summary>
        public static void Start()
        {
            if (Program.Setting.WebServer.UrlPrefixes.Length == 0)
            {
                Logger.Warn($"{nameof(Program.Setting.WebServer.UrlPrefixes)}为空，将不启动网页服务器");
                return;
            }
            _server = new((option) => Program.Setting.WebServer.UrlPrefixes.ToList().ForEach((url) => option.AddUrlPrefix(url)));
            _server.WithStaticFolder("/", "dist", Program.Setting.WebServer.DisableFilesHotUpdate);
            _server.HandleHttpException(Handle404);
            _thread = new Thread(() => _server.RunAsync().Wait()) { IsBackground = true };
            _thread.Start();
        }

        /// <summary>
        /// 关闭服务器
        /// </summary>
        public static void Stop()
        {
            _server?.Dispose();
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
                    _indexHtml ??= File.Exists(_indexHtmlPath) ? File.ReadAllText(_indexHtmlPath) : null;
                }
                else
                {
                    _indexHtml = File.Exists(_indexHtmlPath) ? File.ReadAllText(_indexHtmlPath) : null;
                }
                if (!string.IsNullOrEmpty(_indexHtml))
                {
                    context.Response.StatusCode = 200;
                    await context.SendStringAsync(_indexHtml!, "text/html", Encoding.UTF8);
                    return;
                }
            }
            context.Response.StatusCode = exception.StatusCode;
            await context.SendStandardHtmlAsync(exception.StatusCode);
        }

        private static string? _indexHtmlPath => Path.Combine(Program.Setting.WebServer.Directory, "index.html");

        private static string? _indexHtml;
    }
}
