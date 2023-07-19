using System.Linq;
using Newtonsoft.Json.Linq;
using iPanel.Core.Client;
using iPanel.Core.Connection;
using iPanel.Core.Packets;
using iPanel.Core.Packets.DataBody;
using iPanel.Utils;

namespace iPanel.Core.Service
{
    internal static class ActionsHandler
    {
        public static void Handle(Instance instance, ReceivedPacket packet)
        {
            switch (packet.SubType)
            {
                case "heartbeat":
                    Info? info = packet.Data?.ToObject<Info?>();
                    if (info is null)
                    {
                        instance.Send(new SentPacket("event", "invalid_data", new Reason($"“data”字段为空")).ToString());
                    }
                    else
                    {
                        instance.Info = info;
                    }
                    break;
                default:
                    instance.Send(new SentPacket("event", "invalid_param", new Reason($"所请求的“{packet.Type}”类型不存在或无法调用")).ToString());
                    break;
            }
        }

        /// <summary>
        /// 处理来自控制台的动作数据
        /// </summary>
        /// <param name="console">控制台客户端</param>
        /// <param name="packet">数据包</param>
        public static void Handle(Console console, ReceivedPacket packet)
        {
            bool subAll;
            Instance? instance;
            switch (packet.SubType)
            {
                case "server_start":
                case "server_stop":
                case "server_kill":
                    if (!CheckTarget(console, packet, out subAll, out instance) || instance is null)
                    {
                        console.Send(new SentPacket("event", "invalid_target", new Reason("订阅目标无效")).ToString());
                        break;
                    }
                    Send(console, packet.SubType, null, subAll, instance);
                    break;

                case "customize":
                case "server_input":
                    if (!CheckTarget(console, packet, out subAll, out instance) || instance is null)
                    {
                        console.Send(new SentPacket("event", "invalid_target", new Reason("订阅目标无效")).ToString());
                        break;
                    }
                    if (packet.Data is null || packet.Data.Type != JTokenType.Array && packet.Data.Type != JTokenType.String)
                    {
                        console.Send(new SentPacket("event", "invalid_param", new Reason("“data”字段类型错误")).ToString());
                        break;
                    }
                    Send(console, packet.SubType, packet.Data.Type == JTokenType.String ? new[] { packet.Data } : packet.Data, subAll, instance);
                    break;

                case "instance_list":
                    console.Send(
                        new SentPacket(
                            "event",
                            "list_reply",
                            new JObject() { { "type", "instance" }, { "list", JArray.FromObject(Handler.Instances.Values) } }
                            ).ToString()
                        );
                    break;

                case "console_list":
                    console.Send(
                        new SentPacket(
                            "event",
                            "list_reply",
                            new JObject() { { "type", "console" }, { "list", JArray.FromObject(Handler.Consoles.Values) } }
                            ).ToString()
                        );
                    break;

                case "subscribe":
                    string? target = packet.Data?.ToString();
                    if (!string.IsNullOrEmpty(target) && Handler.Instances.TryGetValue(target!, out Instance? targetInstance))
                    {
                        console.SubscribingTarget = target;
                        console.Send(new SentPacket("event", "target_info", targetInstance.Info, Sender.From(targetInstance)));
                    }
                    else if (target == "*")
                    {
                        console.SubscribingTarget = target;
                    }

                    break;
                default:
                    console.Send(new SentPacket("event", "invalid_param", new Reason($"所请求的“{packet.Type}”类型不存在或无法调用")).ToString());
                    break;
            }
        }

        /// <summary>
        /// 检查目标
        /// </summary>
        /// <param name="console">控制台客户端</param>
        /// <param name="packet">数据包</param>
        /// <returns>检查结果</returns>
        private static bool CheckTarget(Console console, ReceivedPacket packet, out bool subAll, out Instance? instance)
        {
            instance = null;
            subAll = console.SubscribingTarget == "*";
            if (subAll)
            {
                return true;
            }
            if (string.IsNullOrEmpty(console.SubscribingTarget))
            {
                console.Send(new SentPacket("event", "invalid_param", new Reason("未选择目标")).ToString()).Await();
                return false;
            }
            if ((!Handler.Instances.TryGetValue(console.SubscribingTarget!, out instance) || instance is null) && !subAll)
            {
                console.Send(new SentPacket("event", "invalid_param", new Reason("所选目标无效")).ToString()).Await();
                return false;
            }

            return true;
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
                lock (Handler.Instances)
                {
                    Handler.Instances.Values.ToList().ForEach((enumeredInstance) => enumeredInstance?.Send(new SentPacket("action", subType, data, Sender.From(console)).ToString()));
                }
            }
            else
            {
                instance?.Send(new SentPacket("action", subType, data, Sender.From(console)).ToString()).Await();
            }
        }
    }
}