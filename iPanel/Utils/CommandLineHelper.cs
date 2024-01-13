using System;
using System.CommandLine;
using System.IO;
using System.Text.Json;

using iPanel.Core.Models.Settings;
using iPanel.Utils.Json;

using Microsoft.Extensions.Hosting;

namespace iPanel.Utils;

public static class CommandLineHelper
{
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
            throw new FileNotFoundException("\"setting.json\"不存在，现已重新创建，请修改后重启");
        }
        return JsonSerializer.Deserialize<Setting>(
                File.ReadAllText("setting.json"),
                JsonSerializerOptionsFactory.CamelCase
            ) ?? throw new NullReferenceException("文件为空");
    }
}
