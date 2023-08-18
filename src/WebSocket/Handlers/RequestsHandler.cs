using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.Event;
using iPanelHost.Permissons;
using iPanelHost.Utils;
using iPanelHost.WebSocket.Client;
using iPanelHost.WebSocket.Client.Info;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Sys = System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace iPanelHost.WebSocket.Handlers
{
    internal static class RequestsHandler
    {
        /// <summary>
        /// 处理来自实例的请求数据
        /// </summary>
        /// <param name="console">实例客户端</param>
        /// <param name="packet">数据包</param>
        public static void Handle(Instance instance, ReceivedPacket packet)
        {
            Logger.Info($"<{instance.Address}> 收到请求：{packet.SubType}，数据：{packet.Data?.ToString(Formatting.None) ?? "空"}");
            switch (packet.SubType)
            {
                case "heartbeat":
                    FullInfo? info = packet.Data?.ToObject<FullInfo?>();
                    if (info is null)
                    {
                        instance.Send(new InvalidDataPacket("“data”字段为null"));
                    }
                    else
                    {
                        instance.FullInfo = info.Value;
                        lock (MainHandler.Consoles)
                            MainHandler.Consoles
                                .Where((kv) => kv.Value.SubscribingTarget == "*" || kv.Value.SubscribingTarget == instance.UUID)
                                .Select((kv) => kv.Value)
                                .ToList()
                                .ForEach((console) => console.Send(new SentPacket("return", "target_info", instance.FullInfo, Sender.From(instance))));
                    }
                    break;

                default:
                    instance.Send(new InvalidParamPacket($"所请求的“{packet.Type}”类型不存在或无法调用"));
                    break;
            }
        }

        /// <summary>
        /// 处理来自控制台的请求数据
        /// </summary>
        /// <param name="console">控制台客户端</param>
        /// <param name="packet">数据包</param>
        public static void Handle(Console console, ReceivedPacket packet)
        {
            Logger.Info($"<{console.Address}> 收到请求：{packet.SubType}，数据：{packet.Data?.ToString(Formatting.None) ?? "空"}");
            bool subAll;
            Instance? instance;

            switch (packet.SubType)
            {
                case "server_start":
                case "server_stop":
                case "server_kill":
                    if (!CheckTarget(console, out subAll, out instance) && instance is null)
                    {
                        console.Send(new InvalidTargetPacket());
                        break;
                    }
                    Send(console, packet.SubType, null, subAll, instance);
                    break;

                case "server_input":
                    if (!CheckTarget(console, out subAll, out instance) && instance is null)
                    {
                        console.Send(new InvalidTargetPacket());
                        break;
                    }
                    if (packet.Data is null || packet.Data.Type != JTokenType.Array)
                    {
                        console.Send(new InvalidDataPacket("“data”字段类型错误"));
                        break;
                    }
                    Send(console, packet.SubType, packet.Data, subAll, instance);
                    break;

                case "list_instance":
                    console.Send(
                        new SentPacket(
                            "return",
                            "instance_list",
                            JArray.FromObject(MainHandler.Instances.Values)
                            ).ToString()
                        );
                    break;

                case "list_console":
                    console.Send(
                        new SentPacket(
                            "return",
                            "console_list",
                            JArray.FromObject(MainHandler.Consoles.Values)
                            ).ToString()
                        );
                    break;

                case "subscribe":
                    string? target = packet.Data?.ToString();
                    if (target == "*")
                    {
                        console.SubscribingTarget = target;
                    }
                    else if (
                        !string.IsNullOrEmpty(target) &&
                        (instance = MainHandler.Instances.Values.FirstOrDefault((i) => i.InstanceID == target)) is not null)
                    {
                        console.SubscribingTarget = target;
                        console.Send(new SentPacket("return", "target_info", instance.FullInfo, Sender.From(instance)));
                    }
                    else
                    {
                        console.Send(new InvalidTargetPacket());
                    }
                    break;

                case "dissubscribe":
                    console.SubscribingTarget = null;
                    break;

                case "get_target_info":
                    if (!string.IsNullOrEmpty(console.SubscribingTarget) && MainHandler.Instances.TryGetValue(console.SubscribingTarget!, out instance))
                    {
                        console.Send(new SentPacket("return", "target_info", instance.FullInfo, Sender.From(instance)));
                    }
                    else if (console.SubscribingTarget == "*")
                    {
                        Dictionary<string, FullInfo> fullInfos = new();
                        MainHandler.Instances.Values.ToList().ForEach((i) => fullInfos.Add(i.UUID, i.FullInfo));
                        console.Send(new SentPacket("return", "targets_info", fullInfos));
                    }
                    else
                    {
                        console.Send(new InvalidTargetPacket());
                    }
                    break;

                case "get_user_info":
                    if (console.User is null)
                    {
                        console.Send(new OperationResultPacket("内部异常：用户为空"));
                        break;
                    }
                    console.Send(new SentPacket("return", "user_info", new User.PublicUser(console.User)));
                    break;

                case "get_user_dict":
                    if (console.User is null)
                    {
                        console.Send(new OperationResultPacket("内部异常：用户为空"));
                        break;
                    }
                    console.Send(
                        new SentPacket(
                            "return",
                            "user_dict",
                            UserManager.Users
                                .Select((kv) => new KeyValuePair<string, User.PublicUser>(kv.Key, kv.Value))
                                .ToDictionary((kv) => kv.Key, (kv) => kv.Value)
                            ));
                    break;

                default:
                    console.Send(new InvalidParamPacket($"所请求的“{packet.Type}”类型不存在或无法调用"));
                    break;
            }
        }

        /// <summary>
        /// 检查目标
        /// </summary>
        /// <param name="console">控制台客户端</param>
        /// <param name="packet">数据包</param>
        /// <returns>检查结果</returns>
        private static bool CheckTarget(Console console, out bool subAll, out Instance? instance)
        {
            instance = null;
            subAll = console.SubscribingTarget == "*";
            if (subAll)
            {
                return true;
            }
            return
                !string.IsNullOrEmpty(console.SubscribingTarget) &&
                MainHandler.Instances.TryGetValue(console.SubscribingTarget!, out instance) &&
                instance is null;
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="console">控制台客户端</param>
        /// <param name="subType">子类型</param>
        /// <param name="data">数据主体</param>
        /// <param name="subAll">订阅目标为全体</param>
        /// <param name="instance">实例对象</param>
        private static void Send(Console console, string subType, object? data, bool subAll = false, Instance? instance = null)
        {
            if (subAll)
            {
                lock (MainHandler.Instances)
                {
                    MainHandler.Instances.Values.ToList().ForEach((enumeredInstance) => enumeredInstance?.Send(new SentPacket("request", subType, data, Sender.From(console)).ToString()));
                }
            }
            else
            {
                instance?.Send(new SentPacket("request", subType, data, Sender.From(console)).ToString());
            }
        }
    }
}