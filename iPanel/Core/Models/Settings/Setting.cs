using iPanel.Core.Models.Exceptions;
using Swan.Logging;
using System;

#if WINDOWS
using System.Runtime.InteropServices;
#endif

namespace iPanel.Core.Models.Settings;

public class Setting
{

#if WINDOWS
    //引入 Windows API 的消息框函数
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
#endif

    public bool Debug { get; init; }

    public string InstancePassword { get; init; } = creatRandomInstancePassword();

    public WebServerSetting WebServer { get; init; } = new();

    public void Check()
    {
        if (WebServer is null)
            Throw($"{nameof(WebServer)}数据异常");

        if (string.IsNullOrEmpty(InstancePassword))
            Throw($"{nameof(InstancePassword)}为空");

        if (WebServer!.UrlPrefixes is null)
            Throw($"{nameof(WebServer.UrlPrefixes)}为null");

        if (WebServer.UrlPrefixes!.Length == 0)
            Throw($"{nameof(WebServer.UrlPrefixes)}为空。你至少应该设置一个");

        if (string.IsNullOrEmpty(WebServer.Directory))
            Throw($"{nameof(WebServer.Directory)}为空");

        if (string.IsNullOrEmpty(WebServer.Page404))
            Throw($"{nameof(WebServer.Page404)}为空");

        if (WebServer.MaxRequestsPerSecond <= 0)
            Throw($"{nameof(WebServer.MaxRequestsPerSecond)}超出范围");

        if (WebServer.Certificate is null)
            Throw($"{nameof(WebServer.Certificate)}为null");
    }
    private static string creatRandomInstancePassword()
    {
        Random random = new Random(); //创建一个随机数对象

        string password = string.Empty;
        for (int i = 0; i < 9; i++)
        {
            char c = (char)0;
            switch (random.Next(1, 3))
            {
                case 1:
                    c = Convert.ToChar(random.Next('A', 'Z' + 1)); //从大写字母表中随机选取
                    break;
                case 2:
                    c = Convert.ToChar(random.Next('a', 'z' + 1)); //从小写字母表中随机选取
                    break;
                case 3:
                    c = Convert.ToChar(random.Next('0', '9' + 1)); //从数字中随机选取
                    break;
            }
            password += c; // 将字符添加到文本变量中
        }
        return password;
    }
    private static void Throw(string message)
    {
#if WINDOWS
        //错误消息框
        MessageBox(IntPtr.Zero, "解析setting.json遇到问题：" + message, "错误", 0x10 | 0x0);
#endif
        throw new SettingsException(message);
    }
}
