using iPanelHost.Base;
using iPanelHost.Base.Client;
using iPanelHost.Base.Client.Info;
using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.Event;
using iPanelHost.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sys = System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace iPanelHost.Service.Handlers;

public static class RequestsHandler
{
    public static readonly Dictionary<string, Request> ReuqestsDict = new();

    private static readonly Timer _timer = new(1000);

    static RequestsHandler()
    {
        _timer.Elapsed += (_, _) => CheckAllRequest();
        _timer.Start();
    }

    private static string StoreRequest(Request request)
    {
        string id = Sys.Guid.NewGuid().ToString("N");
        ReuqestsDict.Add(id, request);
        return id;
    }

    /// <summary>
    /// 检查所有请求
    /// </summary>
    private static void CheckAllRequest()
    {
        lock (ReuqestsDict)
        {
            foreach (KeyValuePair<string, Request> keyValuePair in ReuqestsDict.ToArray())
            {
                if ((Sys.DateTime.Now - keyValuePair.Value.StartTime).TotalSeconds > 10)
                {
                    if (
                        MainHandler.Consoles.TryGetValue(
                            keyValuePair.Value.CallerUUID,
                            out Console? console
                        )
                    )
                    {
                        console.Send(
                            new OperationResultPacket(keyValuePair.Value.Echo, ResultTypes.Timeout)
                        );
                    }
                    ReuqestsDict.Remove(keyValuePair.Key);
                }
            }
        }
    }
}
