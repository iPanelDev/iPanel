using System;
using System.Runtime.InteropServices;

namespace iPanelHost.Utils
{
    internal static class Win32
    {
        const int STD_INPUT_HANDLE = -10;
        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
        const uint ENABLE_INSERT_MODE = 0x0020;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
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
        /// 启用虚拟终端
        /// </summary>
        public static void EnableVirtualTerminal()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && !Console.IsOutputRedirected)
            {
                IntPtr handle;
                handle = GetStdHandle(STD_OUTPUT_HANDLE);
                GetConsoleMode(handle, out uint outputMode);
                outputMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                SetConsoleMode(handle, outputMode);
            }
        }

        /// <summary>
        /// 设置控制台模式
        /// </summary>
        public static void SetConsoleMode()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && !Console.IsOutputRedirected)
            {
                if (!Program.Setting.Win32Console.AllowQuickEditAndInsert)
                {
                    IntPtr handle = GetStdHandle(STD_INPUT_HANDLE);
                    GetConsoleMode(handle, out uint inputMode);
                    inputMode &= ~ENABLE_QUICK_EDIT_MODE;
                    inputMode &= ~ENABLE_INSERT_MODE;
                    SetConsoleMode(handle, inputMode);
                }

                if (!Program.Setting.Win32Console.AllowWindowClosing)
                {
                    IntPtr windowHandle = FindWindow(null, System.Console.Title);
                    IntPtr closeMenu = GetSystemMenu(windowHandle, IntPtr.Zero);
                    uint SC_CLOSE = 0xF060;
                    RemoveMenu(closeMenu, SC_CLOSE, 0x0);
                }

                Console.Title = $"iPanel Host {Constant.VERSION}";
            }
        }
    }
}