using Fleck;
using System.Threading.Tasks;
using iPanel.Core.Service;
using iPanel.Core.Client;

namespace iPanel.Utils
{
    internal static class Extensions
    {
        /// <summary>
        /// 获取完整地址
        /// </summary>
        /// <param name="client">客户端</param>
        /// <returns>完整地址</returns>
        public static string GetFullAddr(this IWebSocketConnection client)
            => $"{client.ConnectionInfo.ClientIpAddress}:{client.ConnectionInfo.ClientPort}";

        /// <summary>
        /// 作为控制台
        /// </summary>
        public static Console? AsConsole(this IWebSocketConnection client)
        {
            Connections.Consoles.TryGetValue(client.GetFullAddr(), out Console? iPanel);
            return iPanel;
        }

        /// <summary>
        /// 作为实例
        /// </summary>
        public static Instance? AsInstance(this IWebSocketConnection client)
        {
            Connections.Instances.TryGetValue(client.GetFullAddr(), out Instance? instance);
            return instance;
        }

        /// <summary>
        /// 等待
        /// </summary>
        public static void Await(this Task task) => task.GetAwaiter().GetResult();

        /// <summary>
        /// 等待
        /// </summary>
        public static T Await<T>(this Task<T> task) => task.GetAwaiter().GetResult();
    }
}