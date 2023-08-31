using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using iPanelHost.Base.Packets.Event;
using iPanelHost.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace iPanelHost.Server
{
    public class Apis : WebApiController
    {
        public static readonly Encoding UTF8 = new UTF8Encoding(false);

        private static readonly string _intro = new JObject
        {
            { "name",               "iPanel Host" },
            { "version",            Constant.VERSION },
            { "_internal_version",  Constant.InternalVersion },
            { "assembly",           Assembly.GetEntryAssembly()?.ToString() },
            { "framework",          Environment.Version.ToString() },
            { "NOTICE",             "This is an api interface of iPanel Host. For further infomation, please check <https://github.com/iPanelDev/iPanel-Host>." },
        }.ToString();

        /// <summary>
        /// 根
        /// </summary>
        [Route(HttpVerbs.Any, "/")]
        public async Task Root()
        {
            await HttpContext.SendStringAsync(_intro, "text/json", UTF8);
        }

        [Route(HttpVerbs.Any, "/ping")]
        public string Ping() => DateTime.Now.ToString("o");

        /// <summary>
        /// 上传文件
        /// </summary>
        [Route(HttpVerbs.Post, "/upload_1")]
        public async Task SimpleUpload()
        {
            Logger.Info($"<{HttpContext.RemoteEndPoint}> 正在上传文件");
            await FileTransferStation.SimpleUpload(HttpContext);
        }

        /// <summary>
        /// 上传文件流
        /// </summary>
        [Route(HttpVerbs.Post, "/upload_2")]
        public async Task StreamUpload()
        {
            Logger.Info($"<{HttpContext.RemoteEndPoint}> 正在上传文件");
            await FileTransferStation.StreamUpload(HttpContext);
        }

        [Route(HttpVerbs.Any, "/generateCookie")]
        public async Task GenerateCookie()
        {
            HttpContext.Response.SetCookie(new("ipanel-token", Guid.NewGuid().ToString("N")));
            await HttpContext.SendStringAsync(new OperationResultPacket(null, ResultTypes.None).ToString(), "text/json", UTF8);
        }
    }
}