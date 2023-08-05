using iPanelHost.Http;
using iPanelHost.Inputs;
using iPanelHost.WebSocket;
using System;
using System.Linq;

namespace iPanelHost.Utils
{
    internal static class Runtime
    {
        /// <summary>
        /// 开始处理输入
        /// </summary>
        public static void StartHandleInput()
        {
            while (true)
            {
                string? line = Console.ReadLine()?.Trim();
                if (line is null)
                {
                    continue;
                }

                Switcher.Input(line);
            }
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="code">退出代码</param>
        public static void Exit(int code = 0)
        {
            Logger.Warn("退出中...");
            ExitQuietly(code);
        }


        /// <summary>
        /// 安静退出
        /// </summary>
        /// <param name="code">退出代码</param>
        public static void ExitQuietly(int code = 0)
        {
            Handler.Instances.Values.ToList().ForEach((instance) => instance.Close());
            Handler.Consoles.Values.ToList().ForEach((console) => console.Close());
            HttpServer.Stop();
            Environment.Exit(code);
        }
    }
}
