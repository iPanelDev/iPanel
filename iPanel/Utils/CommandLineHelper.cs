using iPanel.Core.Models.Settings;
using iPanel.Core.Service;
using iPanel.Utils.Json;
using System;
using System.CommandLine;
using System.IO;
using System.Text.Json;

namespace iPanel.Utils;

public static class CommandLineHelper
{
    public static RootCommand Create()
    {
        var rootCommnad = new RootCommand();
        var settingsCmd = new Command("setting", "创建设置文件");

        rootCommnad.AddCommand(settingsCmd);
        settingsCmd.SetHandler(WriteSetting);
        rootCommnad.SetHandler(
            async () => await new App(SettingManager.ReadSetting()).StartAsync()
        );

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
}
