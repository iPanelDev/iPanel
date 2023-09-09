using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Utils;
using Newtonsoft.Json;
using System;
using WebSocket4Net;

namespace iPanelHost.Tests;

public static class Utils
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
                    new SentPacket
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
