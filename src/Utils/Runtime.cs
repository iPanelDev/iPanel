using iPanelHost.Server;
using iPanelHost.Interaction;
using iPanelHost.Service.Handlers;
using System;
using System.Linq;

namespace iPanelHost.Utils;

public static class Runtime
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

            Input.ReadLine(line);
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
        MainHandler.Instances.Values.ToList().ForEach((instance) => instance.Close());
        MainHandler.Consoles.Values.ToList().ForEach((console) => console.Close());
        HttpServer.Stop();
        Environment.Exit(code);
    }
}
