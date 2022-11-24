using Fleck;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Timers;

namespace iPanel
{
    internal static class WebSocket
    {
        private static readonly Dictionary<string, string> GUIDs = new Dictionary<string, string>();
        public static readonly Dictionary<string, ConsoleSocket> Consoles = new Dictionary<string, ConsoleSocket>();
        public static readonly Dictionary<string, InstanceSocket> Instances = new Dictionary<string, InstanceSocket>();
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
                        Logger.Info($"{Message} {e}");
                        break;
                    case 2:
                        Logger.Warn($"{Message} {e}");
                        break;
                    case 3:
                        Logger.Error($"{Message} {e}");
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
                    Logger.Connect($"<{ClientUrl}> guid:{GUID}, excepted md5:{GetMD5(GUID + Program.Setting.Password)}");
                    socket.Send(new Packet(
                        "event",
                        "verify_request",
                        GUID,
                        "host"
                        ).ToString());
                    GUIDs.Add(ClientUrl, GUID);
                    Console.Title = $"iPanel - 连接数：{GUIDs.Count}";
                    Timer VerifyTimer = new Timer(5000)
                    {
                        AutoReset = false,
                    };
                    VerifyTimer.Start();
                    VerifyTimer.Elapsed += (sender, e) =>
                    {
                        if (!Consoles.ContainsKey(ClientUrl) && !Instances.ContainsKey(ClientUrl))
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
                    Logger.Disconnect($"<{ClientUrl}>");
                    GUIDs.Remove(ClientUrl);
                    Instances.Remove(ClientUrl);
                    Consoles.Remove(ClientUrl);
                    Console.Title = $"iPanel - 连接数：{GUIDs.Count}";
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
                if (Instances.TryGetValue(TargetUrl, out InstanceSocket TargetSocket))
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
                        Logger.Notice($"<{TargetUrl}> 状态异常，已自动断开");
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
            Console.Title = $"iPanel - 连接数：{GUIDs.Count}";
            string ClientUrl = Socket.ConnectionInfo.ClientIpAddress + ":" + Socket.ConnectionInfo.ClientPort;
            if (!Message.Contains("\"sub_type\":\"heartbeat\""))
            {
                Logger.Recieve($"<{ClientUrl}> {Message}");
            }
            int ClientType = Instances.ContainsKey(ClientUrl) ? 1 : Consoles.ContainsKey(ClientUrl) ? 0 : -1;
            JObject Packet;
            try
            {
                Packet = (JObject)JsonConvert.DeserializeObject(Message.Trim());
            }
            catch (Exception e)
            {
                Logger.Error($"序列化数据包时出现错误(From:{ClientUrl}):{e.Message}\x1b[0m");
                if (ClientType < 0)
                    Socket.Close();
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
                Logger.Notice($"<{ClientUrl}> 未通过验证，已自动断开");
            }
            else if (ClientType == 1 && Instances.ContainsKey(ClientUrl))
                Instances[ClientUrl].LastTime = DateTime.Now;
            if (Type == "api")
            {
                switch (SubType)
                {
                    case "console_verify":
                    case "instance_verify":
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
                                    Instances.Add(ClientUrl, new InstanceSocket()
                                    {
                                        CustomName = CustomName,
                                        WebSocketConnection = Socket,
                                        GUID = GUID
                                    });
                                }
                                Socket.Send(new Packet("response", "verify_success", "验证成功", "host").ToString());
                                Logger.Notice($"<{ClientUrl}> 验证成功:{(SubType == "console_verify" ? "控制台" : "实例")}");
                            }
                            else
                            {
                                Logger.Notice($"<{ClientUrl}> 验证失败:错误的MD5值，已自动断开");
                                Socket.Send(new Packet("response", "verify_failed", "验证失败:错误的MD5值", "host").ToString());
                                Socket.Close();
                            }
                        }
                        break;
                    case "select":
                        if (ClientType == 0 && Consoles.ContainsKey(ClientUrl))
                        {
                            Consoles[ClientUrl].SelectTarget = DataStr;
                        }
                        break;
                    case "list":
                        Socket.Send(new Packet("response", "list", Instances.Values, "host").ToString());
                        break;
                    case "input":
                    case "start":
                    case "stop":
                    case "kill":
                        if (SubType != "input")
                            Data = null;
                        string Target = (Packet["target"] ?? (Consoles.TryGetValue(ClientUrl, out ConsoleSocket ConsoleSocket) ? ConsoleSocket.SelectTarget : string.Empty)).ToString();
                        foreach (string TargetUrl in Instances.Keys.ToArray())
                        {
                            if (TargetUrl != ClientUrl && Instances.TryGetValue(TargetUrl, out InstanceSocket TargetSocket) && (ClientType == 1 || Target == "all" || Target == TargetSocket.GUID))
                            {
                                TargetSocket.WebSocketConnection.Send(
                                    new Packet(
                                        "execute",
                                        SubType,
                                        Data,
                                        new Dictionary<string, string>(){
                                            {"guid",GUID },
                                            {"type",ClientType == 1 ? "instance" : "console" }}
                                        ).ToString());
                                Logger.Send($"<{ClientUrl}> -> <{TargetUrl}>({SubType})");
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
                        if (SubType == "start" || SubType == "exit")
                            Data = null;
                        foreach (string TargetUrl in Consoles.Keys.ToArray())
                        {
                            if (TargetUrl != ClientUrl && Consoles.TryGetValue(TargetUrl, out ConsoleSocket TargetSocket) && (TargetSocket.SelectTarget == "all" || TargetSocket.SelectTarget == GUID))
                            {
                                TargetSocket.WebSocketConnection.Send(
                                    new Packet(
                                        "event",
                                        SubType,
                                        Data,
                                        new Dictionary<string, string>(){
                                            {"guid",GUID },
                                            {"type","instance"}}
                                        ).ToString());
                                Logger.Send($"<{ClientUrl}> -> <{TargetUrl}>({SubType})");
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
