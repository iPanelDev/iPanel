using iPanel.Core.Connection;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace iPanel.Utils
{
    internal static class Runtime
    {
        const int STD_INPUT_HANDLE = -10;
        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
        const uint ENABLE_INSERT_MODE = 0x0020;

        [DllImport("user32.dll")]
        extern static IntPtr FindWindow(string? lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        extern static IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);

        [DllImport("user32.dll")]
        extern static IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        /// <summary>
        /// 设置控制台
        /// </summary>
        public static void SetConsole()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                IntPtr handle = GetStdHandle(STD_OUTPUT_HANDLE);
                GetConsoleMode(handle, out uint outputMode);
                outputMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                SetConsoleMode(handle, outputMode);
                handle = GetStdHandle(STD_INPUT_HANDLE);
                GetConsoleMode(handle, out uint inputMode);
                inputMode &= ~ENABLE_QUICK_EDIT_MODE;
                inputMode &= ~ENABLE_INSERT_MODE;
                SetConsoleMode(handle, inputMode);
                IntPtr windowHandle = FindWindow(null, System.Console.Title);
                IntPtr closeMenu = GetSystemMenu(windowHandle, IntPtr.Zero);
                uint SC_CLOSE = 0xF060;
                RemoveMenu(closeMenu, SC_CLOSE, 0x0);
                Console.Title = "iPanel " + Program.VERSION;
            }

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.OutputEncoding = Encoding.UTF8;
            Console.CancelKeyPress += OnCancel;
        }

        /// <summary>
        /// 开始处理输入
        /// </summary>
        public static void StartHandleInput()
        {
            while (true)
            {
                HandleInput(Console.ReadLine());
            }
        }

        /// <summary>
        /// 处理输入
        /// </summary>
        private static void HandleInput(string? line)
        {
            switch (line)
            {
                case "list":
                    Logger.Info($"当前有{Handler.Consoles.Count}个控制台和{Handler.Instances.Count}个面板在线");
                    Handler.Consoles.Keys.ToList().ForEach((key) =>
                    {
                        Logger.Info($"控制台\t{key,-18}\t{Handler.Consoles[key].CustomName}");
                    });
                    Handler.Instances.Keys.ToList().ForEach((key) =>
                    {
                        Logger.Info($"面板\t{key,-18}\t{Handler.Instances[key].CustomName}");
                    });
                    Logger.Info("");
                    break;

                case "cls":
                case "clear":
                    Console.Clear();
                    break;

                case "exit":
                    Exit();
                    break;

                case null:
                    break;

                default:
                    Logger.Warn("未知的命令。");
                    break;
            }
        }

        /// <summary>
        /// 上一次触发时间
        /// </summary>
        private static DateTime _lastTime = new(1, 1, 1);

        /// <summary>
        /// 按下取消键时触发
        /// </summary>
        private static void OnCancel(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            if ((DateTime.Now - _lastTime).TotalSeconds > 1)
            {
                Logger.Warn("请在1s内再次按下`Ctrl`+`C`以退出。");
                _lastTime = DateTime.Now;
            }
            Exit();
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="code">退出代码</param>
        public static void Exit(int code = 0)
        {
            Handler.Instances.Values.ToList().ForEach((instance) => instance.Close());
            Handler.Consoles.Values.ToList().ForEach((console) => console.Close());
            Environment.Exit(code);
        }
    }
}
