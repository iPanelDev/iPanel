using Fleck;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serein.Base;
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
            public string GUID;
            public int Type = -1;
            public string Name = string.Empty;
            public Info Info = null;
        }
        private static IDictionary<string, Socket> Sockets = new Dictionary<string, Socket>();
        private static List<string> VerifidClientUrl = new List<string>();
        private static WebSocketServer server;
        private static List<Info> Infos = new List<Info>();

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
                    socket.Send(new Packet("verify", "request", GUID, "host", ClientUrl).ToString());
                    Sockets.Add(
                        ClientUrl,
                        new Socket()
                        {
                            GUID = GUID,
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
                            socket.Send(new Packet("notice", "verify_timeout", "验证超时", "host", ClientUrl).ToString());
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
                    if (!message.Contains("\"type\":\"heartbeat\""))
                    {
                        Console.WriteLine($"\x1b[92m[↓]\x1b[0m<{ClientUrl}> {message}");
                    }
                    if (Sockets.Keys.Contains(ClientUrl))
                    {
                        JObject packet;
                        try
                        {
                            packet = (JObject)JsonConvert.DeserializeObject(message.Trim());
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"\x1b[91m[Error]序列化数据包时出现错误(From:{ClientUrl}):{e.Message}\x1b[0m");
                            return;
                        }
                        int ClientType = Sockets[ClientUrl].Type;
                        string Name = Sockets[ClientUrl].Name;
                        string GUID = Sockets[ClientUrl].GUID;
                        string Type = (packet["type"] ?? "").ToString();
                        string SubType = (packet["sub_type"] ?? "").ToString();
                        string From = (packet["from"] ?? "").ToString();
                        string Target = (packet["target"] ?? "").ToString();
                        JToken Data = packet["data"];
                        if (ClientType == -1 && Type != "verify")
                        {
                            socket.Send(new Packet("notice", "unverified", "需要通过验证", "host", ClientUrl).ToString());
                            Console.WriteLine($"\x1b[93m[﹗]\x1b[0m<{ClientUrl}> 你需要通过验证({Type}:{SubType})");
                        }
                        switch (Type)
                        {
                            case "verify":
                                if (!VerifidClientUrl.Contains(ClientUrl))
                                {
                                    Sockets[ClientUrl].Name = string.IsNullOrEmpty(From) || From.ToLower() == "host" ?
                                        GUID : From;
                                    Console.WriteLine($"\x1b[93m[﹗]\x1b[0m<{ClientUrl}> 自定义名称:{Sockets[ClientUrl].Name}");
                                    if ((Data ?? "").ToString() == GetMD5(
                                            Sockets[ClientUrl].GUID +
                                            (Program.Args.Count > 0 ? Program.Args[0] : "pwd")))
                                    {
                                        switch (SubType)
                                        {
                                            case "server_reply":
                                                Sockets[ClientUrl].Type = 0;
                                                VerifidClientUrl.Add(ClientUrl);
                                                socket.Send(new Packet("notice", "verify_success", "验证成功:服务器", "host", ClientUrl).ToString());
                                                Console.WriteLine($"\x1b[93m[﹗]\x1b[0m<{ClientUrl}> 验证成功:服务器");
                                                break;
                                            case "console_reply":
                                                Sockets[ClientUrl].Type = 1;
                                                VerifidClientUrl.Add(ClientUrl);
                                                socket.Send(new Packet("notice", "verify_success", "验证成功:控制台", "host", ClientUrl).ToString());
                                                Console.WriteLine($"\x1b[93m[﹗]\x1b[0m<{ClientUrl}> 验证成功:控制台");
                                                break;
                                            default:
                                                socket.Send(new Packet("notice", "verify_failed", "验证失败:无效的客户端类型", "host", ClientUrl).ToString());
                                                Console.WriteLine($"\x1b[93m[﹗]\x1b[0m<{ClientUrl}> 验证失败:无效的客户端类型");
                                                socket.Close();
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"\x1b[95m[﹗]\x1b[0m<{ClientUrl}> 验证失败:错误的MD5值");
                                        socket.Send(new Packet("notice", "verify_failed", "验证失败:错误的MD5值", "host", ClientUrl).ToString());
                                        socket.Close();
                                    }
                                }
                                break;
                            case "input":
                                foreach (string TargetUrl in Sockets.Keys.ToArray())
                                {
                                    if (TargetUrl != ClientUrl && Sockets.TryGetValue(TargetUrl, out Socket TargetSocket) && (ClientType == 0 || Target == "all" || Target == TargetSocket.GUID))
                                    {
                                        TargetSocket.WebSocketConnection.Send(
                                            new Packet(
                                                "input",
                                                ClientType == 0 ? "server" : "console",
                                                (Data ?? "").ToString(),
                                                GUID,
                                                TargetSocket.GUID
                                                ).ToString());
                                        Console.WriteLine($"\x1b[96m[↑]\x1b[0m<{ClientUrl}> -> <{TargetUrl}>({SubType.ToLower()})");
                                    }
                                }
                                break;
                            case "output":
                                if (ClientType == 1)
                                {
                                    socket.Send(new Packet("notice", "permission_denied", "没有权限", "host", ClientUrl).ToString());
                                    Console.WriteLine($"\x1b[93m[﹗]\x1b[0m<{ClientUrl}> 没有权限({packet.Type}:{SubType})");
                                    break;
                                }
                                foreach (string TargetUrl in Sockets.Keys.ToArray())
                                {
                                    if (TargetUrl != ClientUrl && Sockets.TryGetValue(TargetUrl, out Socket TargetSocket))
                                    {
                                        TargetSocket.WebSocketConnection.Send(
                                            new Packet(
                                                "output",
                                                TargetSocket.Type == 0 ? "origin" : "colored",
                                                Log.ColorLog((Data ?? "").ToString(), TargetSocket.Type == 0 ? 0 : 3),
                                                GUID,
                                                TargetSocket.GUID
                                                ).ToString());
                                        Console.WriteLine($"\x1b[96m[↑]\x1b[0m<{ClientUrl}> -> <{TargetUrl}>({SubType.ToLower()})");
                                    }
                                }
                                break;
                            case "heartbeat":
                                if (SubType != "response")
                                {
                                    break;
                                }
                                if (ClientType == 0)
                                {
                                    try
                                    {
                                        Sockets[ClientUrl].Info = Data.ToObject<Info>();
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine($"\x1b[33m[Warn]序列化数据包\"data\"时出现错误(From:{ClientUrl}):{e.Message}\x1b[0m");
                                    }
                                }
                                break;
                            default:
                                socket.Send(new Packet("notice", "unknown_type", "未知的数据包类型", "host", ClientUrl).ToString());
                                Console.WriteLine($"\x1b[93m[﹗]\x1b[0m<{ClientUrl}> 未知的数据包类型({packet.Type}:{SubType})");
                                break;
                        }
                    }
                };
            });
            Timer HeartBeat = new Timer(5000)
            {
                AutoReset = true
            };
            HeartBeat.Elapsed += (sender, e) =>
            {
                Infos.Clear();
                foreach (Socket socket in Sockets.Values.ToArray())
                {
                    if (socket.Type == 0 && socket.Info != null)
                    {
                        socket.Info.GUID = socket.GUID;
                        Infos.Add(socket.Info);
                    }
                }
                foreach (string TargetUrl in Sockets.Keys.ToArray())
                {
                    if (Sockets.TryGetValue(TargetUrl, out Socket TargetSocket))
                    {
                        TargetSocket.WebSocketConnection.Send(
                            new Packet(
                                "heartbeat",
                                "request",
                                "",
                                "host",
                                TargetSocket.GUID
                                ).ToString());
                        if (TargetSocket.Type == 1)
                        {
                            TargetSocket.WebSocketConnection.Send(
                            new Packet(
                                "heartbeat",
                                "info",
                                Infos,
                                "host",
                                TargetSocket.GUID
                                ).ToString());
                        }
                    }
                }
            };
            HeartBeat.Start();
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
