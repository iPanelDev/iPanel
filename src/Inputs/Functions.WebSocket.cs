using iPanelHost.WebSocket;
using iPanelHost.WebSocket.Client;
using iPanelHost.Base.Packets;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Utils;
using Sharprompt;
using System.Collections.Generic;
using System.Linq;

namespace iPanelHost.Inputs
{
    internal static partial class Funcions
    {
        /// <summary>
        /// 断开连接
        /// </summary>
        public static void Disconnect()
        {
            if (Handler.Instances.Count == 0)
            {
                Logger.Warn("当前没有实例在线");
                return;
            }
            try
            {
                KeyValuePair<string, Instance> keyValuePair = Prompt.Select<KeyValuePair<string, Instance>>("请选择要断开的实例", Handler.Instances.ToList(), textSelector: (kv) => $"{kv.Value.Address}\t自定义名称：{kv.Value.CustomName ?? "未知名称"}");
                keyValuePair.Value?.Send(new SentPacket("event", "disconnection", new Result("被用户手动断开")));
                keyValuePair.Value?.Close();
                return;
            }
            catch (PromptCanceledException)
            {
                return;
            }
            catch
            {
                Logger.Warn("所选实例无效");
            }
        }

        /// <summary>
        /// 更改自定义名称
        /// </summary>
        public static void ChangeCustomName()
        {
            if (Handler.Instances.Count == 0)
            {
                Logger.Warn("当前没有实例在线");
                return;
            }
            try
            {
                KeyValuePair<string, Instance> keyValuePair = Prompt.Select<KeyValuePair<string, Instance>>("请选择要修改名称的实例", Handler.Instances.ToList(), textSelector: (kv) => $"{kv.Value.Address}\t自定义名称：{kv.Value.CustomName ?? "未知名称"}");
                if (Handler.Instances.ContainsKey(keyValuePair.Key))
                {
                    string? newName = Prompt.Input<string>("请输入新的名称", null, Handler.Instances[keyValuePair.Key].CustomName);
                    if (Handler.Instances.ContainsKey(keyValuePair.Key))
                    {
                        Handler.Instances[keyValuePair.Key].CustomName = newName;
                        Logger.Info("实例修改成功");
                        return;
                    }

                }
            }
            catch (PromptCanceledException)
            {
                return;
            }
            catch
            {
                return;
            }
            Logger.Warn("所选实例无效");
        }
    }
}