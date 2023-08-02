using Fleck;
using System.Threading.Tasks;
using iPanelHost.WebSocket.Client;

namespace iPanelHost.Utils
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
        /// 等待
        /// </summary>
        public static void Await(this Task task) => task.GetAwaiter().GetResult();

        /// <summary>
        /// 等待
        /// </summary>
        public static T Await<T>(this Task<T> task) => task.GetAwaiter().GetResult();
    }
}