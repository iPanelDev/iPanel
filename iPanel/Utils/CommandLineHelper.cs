using iPanel.Core.Models.Settings;
using iPanel.Utils.Json;
using Microsoft.Extensions.Hosting;
using System;
using System.CommandLine;
using System.IO;
using System.Text.Json;

#if WINDOWS
using System.Runtime.InteropServices;
#endif

namespace iPanel.Utils;

public static class CommandLineHelper
{

#if WINDOWS
    //引入 Windows API 的消息框函数
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
#endif

    public static RootCommand Create()
    {
        var rootCommnad = new RootCommand();
        var settingsCmd = new Command("setting", "创建设置文件");

        rootCommnad.AddCommand(settingsCmd);
        settingsCmd.SetHandler(WriteSetting);
        rootCommnad.SetHandler(() => new AppBuilder(ReadSetting()).Build().Run());

        return rootCommnad;
    }

    private static void WriteSetting() =>
        File.WriteAllText(
            "setting.json",
            JsonSerializer.Serialize(
                new Setting(),
                options: new(JsonSerializerOptionsFactory.CamelCase) { WriteIndented = true }
            )
        );

    public static Setting ReadSetting()
    {
        if (!File.Exists("setting.json"))
        {
            WriteSetting();
#if WINDOWS
            //错误消息框
            MessageBox(IntPtr.Zero, "\"setting.json\"不存在，现已重新创建，请修改后重启", "错误", 0x10 | 0x0);
#endif
            throw new FileNotFoundException("\"setting.json\"不存在，现已重新创建，请修改后重启");
        }
        Setting? settingfilevalue = null;
        try
        {
            settingfilevalue = JsonSerializer.Deserialize<Setting>(File.ReadAllText("setting.json"), JsonSerializerOptionsFactory.CamelCase);
            if (settingfilevalue == null)
            {
                throw new NullReferenceException("文件为空");
            }
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine("读取设置文件发生异常: " + ex.Message);
#if WINDOWS
            //错误消息框
            MessageBox(IntPtr.Zero, "读取设置文件发生异常:\r\n" + ex.Message, "错误", 0x10 | 0x0);
#endif
            throw;
        }
        return settingfilevalue;
    }
}
