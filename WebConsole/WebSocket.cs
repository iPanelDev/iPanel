using Fleck;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Timers;

namespace WebConsole
{
    internal static class WebSocket
    {
        private class Socket
        {
            public IWebSocketConnection WebSocketConnection;
            public string WebSocketId;
            public int Type = -1;
        }
        private static IDictionary<string, Socket> Sockets = new Dictionary<string, Socket>();
        private static List<string> VerifidClientUrl = new List<string>();
        private static WebSocketServer server;
        public static void Start()
        {
            FleckLog.LogAction = (Level, Message, e) =>
            {
                switch ((int)Level)
                {
                    case 0:
                        //Console.WriteLine($"\x1b[95m[Debug]\x1b[0m{Message} {e}");
                        break;
                    case 1:
                        Console.WriteLine($"\x1b[96m[Info]\x1b[0m{Message} {e}");
                        break;
                    case 2:
                        Console.WriteLine($"\x1b[33m[Warn]{Message} {e}\x1b[0m");
                        break;
                    case 3:
                        Console.WriteLine($"\x1b[91m[Error]{Message} {e}\x1b[0m");
                        break;
                }
            };
            server = new WebSocketServer("ws://0.0.0.0:30000")
            {
                RestartAfterListenError = true
            };
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    string ClientUrl = socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort;
                    string GUID = Guid.NewGuid().ToString().Replace("-", string.Empty);
                    Console.WriteLine($"\x1b[36m[＋]\x1b[0m<{ClientUrl}> guid:{GUID}, md5:{GetMD5(GUID + (Program.Args.Count > 0 ? Program.Args[0] : "pwd"))}");
                    socket.Send(JsonConvert.SerializeObject(new Packet("request", "verify", GUID, "host")));
                    Sockets.Add(
                        ClientUrl,
                        new Socket()
                        {
                            WebSocketId = GUID,
                            WebSocketConnection = socket
                        }
                        );
                    Console.Title = $"WebConsole - Serein ({Sockets.Count})";
                    Timer VerifyTimer = new Timer(5000)
                    {
                        AutoReset = false,
                    };
                    VerifyTimer.Start();
                    VerifyTimer.Elapsed += (sender, e) =>
                    {
                        if (!VerifidClientUrl.Contains(ClientUrl))
                        {
                            socket.Send(JsonConvert.SerializeObject(new Packet("notice", "verify_timeout", "验证超时", "host")));
                            socket.Close();
                        }
                        VerifyTimer.Dispose();
                    };
                };
                socket.OnClose = () =>
                {
                    Console.Title = $"WebConsole - Serein ({Sockets.Count})";
                    string ClientUrl = socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort;
                    Console.WriteLine($"\x1b[36m[－]\x1b[0m<{ClientUrl}>");
                    Sockets.Remove(ClientUrl);
                    VerifidClientUrl.Remove(ClientUrl);
                };
                socket.OnMessage = message =>
                {
                    Console.Title = $"WebConsole - Serein ({Sockets.Count})";
                    string ClientUrl = socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort;
                    Console.WriteLine($"\x1b[92m[↓]\x1b[0m<{ClientUrl}> {message}");
                    if (Sockets.Keys.Contains(ClientUrl))
                    {
                        Packet packet = new Packet();
                        try
                        {
                            packet = JsonConvert.DeserializeObject<Packet>(message.Trim());
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"\x1b[91m[Error]序列化数据包时出现错误(From:{ClientUrl}):{e}\x1b[0m");
                        }
                        switch (packet.Type)
                        {
                            case "reply":
                                if (!VerifidClientUrl.Contains(ClientUrl) && packet.SubType == "verify")
                                {
                                    if (packet.Data == GetMD5(
                                            Sockets[ClientUrl].WebSocketId +
                                            (Program.Args.Count > 0 ? Program.Args[0] : "pwd"))||
                                            packet.Data=="test")
                                    {
                                        switch (packet.From.ToLower())
                                        {
                                            case "server":
                                                Sockets[ClientUrl].Type = 0;
                                                VerifidClientUrl.Add(ClientUrl);
                                                socket.Send(JsonConvert.SerializeObject(new Packet("notice", "verify_success", "验证成功:服务器", "host")));
                                                Console.WriteLine($"\x1b[93m[﹗]\x1b[0m<{ClientUrl}> 验证成功:服务器");
                                                break;
                                            case "console":
                                                Sockets[ClientUrl].Type = 1;
                                                VerifidClientUrl.Add(ClientUrl);
                                                socket.Send(JsonConvert.SerializeObject(new Packet("notice", "verify_success", "验证成功:控制台", "host")));
                                                Console.WriteLine($"\x1b[93m[﹗]\x1b[0m<{ClientUrl}> 验证成功:控制台");
                                                break;
                                            default:
                                                socket.Send(JsonConvert.SerializeObject(new Packet("notice", "verify_failed", "验证失败:无效的客户端类型", "host")));
                                                Console.WriteLine($"\x1b[93m[﹗]\x1b[0m<{ClientUrl}> 验证失败:无效的客户端类型");
                                                socket.Close();
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"\x1b[95m[﹗]\x1b[0m<{ClientUrl}> 验证失败:错误的MD5值");
                                        socket.Send(JsonConvert.SerializeObject(new Packet("notice", "verify_failed", "验证失败:错误的MD5值", "host")));
                                        socket.Close();
                                    }
                                }
                                break;
                            case "msg":
                                int ClientType = Sockets[ClientUrl].Type;
                                if (ClientType == 0)
                                {
                                    if (packet.SubType.ToLower() == "output" ||
                                        packet.SubType.ToLower() == "input")
                                    {
                                        foreach (string TargetUrl in Sockets.Keys.ToArray())
                                        {
                                            if (Sockets.TryGetValue(TargetUrl, out Socket TargetSocket))
                                            {
                                                TargetSocket.WebSocketConnection.Send(
                                                    JsonConvert.SerializeObject(new Packet(
                                                        "msg", 
                                                        "server_"+packet.SubType.ToLower(),
                                                        packet.Data,
                                                        ClientUrl
                                                        )));
                                                Console.WriteLine($"\x1b[96m[↑]\x1b[0m<{ClientUrl}> -> <{TargetUrl}>({packet.SubType.ToLower()})");
                                            }
                                        }
                                    }
                                }
                                else if (ClientType == 1)
                                {
                                    if (packet.SubType.ToLower() == "input")
                                    {
                                        foreach (string TargetUrl in Sockets.Keys.ToArray())
                                        {
                                            if (
                                                Sockets.TryGetValue(TargetUrl, out Socket TargetSocket) &&
                                                TargetSocket.Type == 0
                                            )
                                            {
                                                TargetSocket.WebSocketConnection.Send(
                                                    JsonConvert.SerializeObject(new Packet(
                                                        "msg",
                                                        "console_" + packet.SubType.ToLower(),
                                                        packet.Data,
                                                        ClientUrl
                                                        )));
                                                Console.WriteLine($"\x1b[96m[↑]\x1b[0m<{ClientUrl}> -> <{TargetUrl}>({packet.SubType.ToLower()})");
                                            }
                                        }
                                    }
                                }
                                break;
                            default:
                                socket.Send(JsonConvert.SerializeObject(new Packet("notice", "unknown_type", "未知的数据包类型", "host")));
                                Console.WriteLine($"\x1b[93m[﹗]\x1b[0m<{ClientUrl}> 未知的数据包类型({packet.Type}:{packet.SubType})");
                                break;
                        }
                    }
                };
            });
        }

        public static string GetMD5(string myString)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = Encoding.UTF8.GetBytes(myString);
            byte[] targetData = md5.ComputeHash(fromData);
            string Result = string.Empty;
            for (int i = 0; i < targetData.Length; i++)
            {
                Result += targetData[i].ToString("x2");
            }
            return Result;
        }
    }
}
