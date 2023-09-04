using iPanelHost.Base;
using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Server;
using iPanelHost.Service;
using iPanelHost.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using WebSocket4Net;
using Xunit;
using Xunit.Abstractions;

namespace iPanelHost.Tests;

public class Utils
{
    public static WebSocket CreateConsoleWebSocket(string user, string password)
    {
        WebSocket webSocket = new("ws://127.0.0.1:30000/ws");
        webSocket.MessageReceived += Verify;
        return webSocket;

        void Verify(object? sender, MessageReceivedEventArgs e)
        {
            ReceivedPacket? packet = JsonConvert.DeserializeObject<ReceivedPacket>(e.Message);
            if (packet?.SubType == "verify_request")
            {
                VerifyRequest verifyRequest = packet.Data!.ToObject<VerifyRequest>()!;
                string token = General.GetMD5(verifyRequest.UUID + user + password);

                webSocket.Send(
                    new SentPacket()
                    {
                        Type = "request",
                        SubType = "verify",
                        Data = new VerifyBody
                        {
                            InstanceID = Guid.NewGuid().ToString("N"),
                            ClientType = "console",
                            Token = token
                        }
                    }.ToString()
                );
            }
        }
    }
}
