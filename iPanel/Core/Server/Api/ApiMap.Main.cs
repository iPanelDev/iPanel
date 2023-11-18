using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using System.Net;
using System.Threading.Tasks;

namespace iPanel.Core.Server.Api;

public partial class ApiMap : WebApiController
{
    private readonly App _app;

    public ApiMap(App app)
    {
        _app = app;
    }

    [Route(HttpVerbs.Get, "/")]
    public async Task Root()
    {
        await HttpContext.SendJsonAsync("Hello world. :)", HttpStatusCode.OK);
    }

    [Route(HttpVerbs.Get, "/meta/version")]
    public async Task GetVersion()
    {
        await HttpContext.SendJsonAsync(Constant.Version, HttpStatusCode.OK);
    }

    [Route(HttpVerbs.Get, "/meta/os")]
    public async Task GetOS()
    {
        await HttpContext.SendJsonAsync(Environment.OSVersion, HttpStatusCode.OK);
    }
}
