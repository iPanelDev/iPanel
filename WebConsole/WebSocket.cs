using Fleck;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private static Dictionary<string, string> GUIDs = new Dictionary<string, string>();
        public static Dictionary<string, ConsoleSocket> Consoles = new Dictionary<string, ConsoleSocket>();
        public static Dictionary<string, PanelSocket> Panels = new Dictionary<string, PanelSocket>();
        private static WebSocketServer Server;

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
            Server = new WebSocketServer(Program.Setting.Addr)
            {
                RestartAfterListenError = true
            };
            Server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    string ClientUrl = socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort;
                    string GUID = Guid.NewGuid().ToString().Replace("-", string.Empty);
                    Console.WriteLine($"\x1b[36m[＋]\x1b[0m<{ClientUrl}> guid:{GUID}, md5:{GetMD5(GUID + Program.Setting.Password)}");
                    socket.Send(new Packet(
                        "event",
                        "verify_request",
                        GUID,
                        "host"
                        ).ToString());
                    GUIDs.Add(ClientUrl, GUID);
                    Console.Title = $"WebConsole - Serein ({GUIDs.Count})";
                    Timer VerifyTimer = new Timer(5000)
                    {
                        AutoReset = false,
                    };
                    VerifyTimer.Start();
                    VerifyTimer.Elapsed += (sender, e) =>
                    {
                        if (!Consoles.ContainsKey(ClientUrl) && !Panels.ContainsKey(ClientUrl))
                        {
                            socket.Send(new Packet("response", "verify_timeout", "验证超时", "host").ToString());
                            socket.Close();
                        }
                        VerifyTimer.Dispose();
                    };
                };
                socket.OnClose = () =>
                {
                    string ClientUrl = socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort;
                    Console.WriteLine($"\x1b[36m[－]\x1b[0m<{ClientUrl}>");
                    GUIDs.Remove(ClientUrl);
                    Panels.Remove(ClientUrl);
                    Consoles.Remove(ClientUrl);
                    Console.Title = $"WebConsole - Serein ({GUIDs.Count})";
                };
                socket.OnMessage = message => Receive(socket, message);
            });
            Timer HeartBeat = new Timer(5000)
            {
                AutoReset = true
            };
            HeartBeat.Elapsed += HeartBeatFunc;
            HeartBeat.Start();
        }

        public static void HeartBeatFunc(object sender, ElapsedEventArgs e)
        {
            foreach (string TargetUrl in GUIDs.Keys)
            {
                if (Panels.TryGetValue(TargetUrl, out PanelSocket TargetSocket))
                {
                    if ((DateTime.Now - TargetSocket.LastTime).TotalSeconds > 15)
                    {
                        TargetSocket.WebSocketConnection.Send(
                            new Packet(
                                "event",
                                "close",
                                "状态异常",
                                "host"
                                ).ToString());
                        Console.WriteLine($"\x1b[93m[﹗]\x1b[0m<{TargetUrl}> 状态异常，请重新连接");
                        TargetSocket.WebSocketConnection.Close();
                    }
                    else
                    {
                        TargetSocket.WebSocketConnection.Send(
                        new Packet(
                            "heartbeat",
                            "info",
                            null,
                            "host"
                            ).ToString());
                    }
                }
            }
        }

        private static void Receive(IWebSocketConnection Socket, string Message)
        {
            Console.Title = $"WebConsole - Serein ({GUIDs.Count})";
            string ClientUrl = Socket.ConnectionInfo.ClientIpAddress + ":" + Socket.ConnectionInfo.ClientPort;
            if (!Message.Contains("\"sub_type\":\"heartbeat\""))
            {
                Console.WriteLine($"\x1b[92m[↓]\x1b[0m<{ClientUrl}> {Message}");
            }
            int ClientType = Panels.ContainsKey(ClientUrl) ? 1 : Consoles.ContainsKey(ClientUrl) ? 0 : -1;
            JObject Packet;
            try
            {
                Packet = (JObject)JsonConvert.DeserializeObject(Message.Trim());
            }
            catch (Exception e)
            {
                Console.WriteLine($"\x1b[91m[Error]序列化数据包时出现错误(From:{ClientUrl}):{e.Message}\x1b[0m");
                if (ClientType < 0)
                {
                    Socket.Close();
                }
                return;
            }
            string GUID = GUIDs[ClientUrl];
            string Type = (Packet["type"] ?? string.Empty).ToString();
            string SubType = (Packet["sub_type"] ?? "").ToString();
            object Data = Packet["data"];
            string DataStr = (Packet["data"] ?? string.Empty).ToString();
            if (ClientType == -1 && Type != "api" && !SubType.Contains("verify"))
            {
                Socket.Send(new Packet("response", "unverified", "需要通过验证", "host").ToString());
                Socket.Close();
                Console.WriteLine($"\x1b[93m[﹗]\x1b[0m<{ClientUrl}> 需要通过验证");
            }
            else if (ClientType == 1 && Panels.ContainsKey(ClientUrl))
            {
                Panels[ClientUrl].LastTime = DateTime.Now;
            }
            if (Type == "api")
            {
                switch (SubType)
                {
                    case "console_verify":
                    case "panel_verify":
                        if (ClientType == -1)
                        {
                            if (DataStr == GetMD5(GUID + Program.Setting.Password))
                            {
                                string CustomName = (Packet["custom_name"] ?? "unknown").ToString();
                                if (SubType == "console_verify")
                                {
                                    Consoles.Add(ClientUrl, new ConsoleSocket()
                                    {
                                        CustomName = CustomName,
                                        WebSocketConnection = Socket,
                                        GUID = GUID
                                    });
                                }
                                else
                                {
                                    Panels.Add(ClientUrl, new PanelSocket()
                                    {
                                        CustomName = CustomName,
                                        WebSocketConnection = Socket,
                                        GUID = GUID
                                    });
                                }
                                Socket.Send(new Packet("response", "verify_success", "验证成功", "host").ToString());
                                Console.WriteLine($"\x1b[93m[﹗]\x1b[0m<{ClientUrl}> 验证成功:{(SubType == "panel_reply" ? "服务器" : "控制台")}");
                            }
                            else
                            {
                                Console.WriteLine($"\x1b[95m[﹗]\x1b[0m<{ClientUrl}> 验证失败:错误的MD5值");
                                Socket.Send(new Packet("response", "verify_failed", "验证失败:错误的MD5值", "host").ToString());
                                Socket.Close();
                            }
                        }
                        break;
                    case "select":
                        if (ClientType == 0 && Consoles.ContainsKey(ClientUrl))
                        {
                            Consoles[ClientUrl].Select = DataStr;
                        }
                        break;
                    case "list":
                        Socket.Send(new Packet("response", "list", Panels.Values, "host").ToString());
                        break;
                    case "input":
                    case "start":
                    case "stop":
                    case "kill":
                        if (SubType != "input")
                        {
                            Data = null;
                        }
                        string Target = (Packet["target"] ?? (Consoles.TryGetValue(ClientUrl, out ConsoleSocket ConsoleSocket) ? ConsoleSocket.Select : string.Empty)).ToString();
                        foreach (string TargetUrl in Panels.Keys.ToArray())
                        {
                            if (TargetUrl != ClientUrl && Panels.TryGetValue(TargetUrl, out PanelSocket TargetSocket) && (ClientType == 1 || Target == "all" || Target == TargetSocket.GUID))
                            {
                                TargetSocket.WebSocketConnection.Send(
                                    new Packet(
                                        "execute",
                                        SubType,
                                        Data,
                                        new Dictionary<string, string>(){
                                            {"guid",GUID },
                                            {"type",ClientType==0?"panel":"console" }}
                                        ).ToString());
                                Console.WriteLine($"\x1b[96m[↑]\x1b[0m<{ClientUrl}> -> <{TargetUrl}>({SubType})");
                            }
                        }
                        break;
                }
            }
            else if (Type == "event")
            {
                switch (SubType)
                {
                    case "start":
                    case "stop":
                    case "exit":
                    case "output":
                    case "input":
                    case "heartbeat":
                        if (new[] { "start", "exit" }.Contains(SubType))
                        {
                            Data = null;
                        }
                        foreach (string TargetUrl in Consoles.Keys.ToArray())
                        {
                            if (TargetUrl != ClientUrl && Consoles.TryGetValue(TargetUrl, out ConsoleSocket TargetSocket) && (TargetSocket.Select == "all" || TargetSocket.Select == GUID))
                            {
                                TargetSocket.WebSocketConnection.Send(
                                    new Packet(
                                        "event",
                                        SubType,
                                        Data,
                                        new Dictionary<string, string>(){
                                            {"guid",GUID },
                                            {"type","panel"}}
                                        ).ToString());
                                Console.WriteLine($"\x1b[96m[↑]\x1b[0m<{ClientUrl}> -> <{TargetUrl}>({SubType})");
                            }
                        }
                        break;
                }
            }
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
