using System;
using System.Linq;
using iPanelHost.Interaction;
using iPanelHost.Server;
using iPanelHost.Server.WebSocket.Handlers;

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

            Input.Parse(line);
        }
    }

    /// <summary>
    /// 退出
    /// </summary>
    /// <param name="code">退出代码</param>
    public static void Exit()
    {
        Logger.Warn("退出中...");
        ExitQuietly(0);
    }

    /// <summary>
    /// 安静退出
    /// </summary>
    /// <param name="code">退出代码</param>
    public static void ExitQuietly(int code)
    {
        HttpServer.Stop();
        Environment.Exit(code);
    }
}
