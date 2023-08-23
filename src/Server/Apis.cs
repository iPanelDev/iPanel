using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace iPanelHost.Server
{
    internal class Apis : WebApiController
    {
        private static readonly Encoding _utf8 = new UTF8Encoding(false);

        private static readonly string _intro = new JObject
        {
            { "name",               "iPanel Host" },
            { "version",            Constant.VERSION },
            { "_internal_version",  Constant.InternalVersion },
            { "assembly",           Assembly.GetEntryAssembly()?.ToString() },
            { "framework",          Environment.Version.ToString() },
            { "NOTICE",             "This is an api interface of iPanel Host. For further infomation, please check <https://github.com/iPanelDev/iPanel-Host>." },
        }.ToString();


        [Route(HttpVerbs.Any, "/")]
        public async Task Root()
        {
            await HttpContext.SendStringAsync(_intro, "text/json", _utf8);
        }

        [Route(HttpVerbs.Any, "/ping")]
        public string Ping() => DateTime.Now.ToString("o");
    }
}