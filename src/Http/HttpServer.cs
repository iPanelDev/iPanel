using EmbedIO;
using iPanelHost.WebSocket;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swan.Logging;

namespace iPanelHost.Http
{
    internal static class HttpServer
    {
        private static WebServer? _server;

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
            _server.WithModule(new WsModule("/ws"));
            _server.WithStaticFolder("/", "dist", Program.Setting.WebServer.DisableFilesHotUpdate);
            _server.HandleHttpException(Handle404);
            _server.RunAsync();
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
