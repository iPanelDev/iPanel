using iPanelHost.Base;
using Newtonsoft.Json;
using Sharprompt;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace iPanelHost.Utils
{
    internal static class Initialization
    {
        /// <summary>
        /// 初始化环境
        /// </summary> 
        public static void InitEnv()
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            CrashInterception.Init();
            Runtime.SetConsoleMode();

            Swan.Logging.Logger.UnregisterLogger<Swan.Logging.ConsoleLogger>();
            Swan.Logging.Logger.RegisterLogger<Logger>();

            Prompt.ThrowExceptionOnCancel = true;
            Prompt.Symbols.Done = new("√", "V");
            Prompt.ColorSchema.PromptSymbol = ConsoleColor.Blue;
            Prompt.ColorSchema.Select = ConsoleColor.Gray;
            Prompt.ColorSchema.Answer = ConsoleColor.Gray;
        }

        /// <summary>
        /// 初始化设置
        /// </summary>
        public static void InitSetting()
        {
            Console.WriteLine(Program.Logo);
            try
            {
                Setting setting = new()
                {
                    WebSocket =
                    {
                        Addr = Prompt.Input<string>(
                            "WebSocket地址",
                            "ws://0.0.0.0:30000",
                            null,
                            new[] {
                                Validators.RegularExpression(@"^wss?://(\w+\.)+\w+(:\d{1,5})?(/.+)?", "WebSocket地址格式不正确"),
                                }),
                        Password = Prompt.Password(
                            "实例连接密码",
                            validators: new[] {
                                Validators.Required("密码不可为空"),
                                Validators.MinLength(3, "密码长度过短"),
                                (obj) => string.IsNullOrWhiteSpace(obj?.ToString()) ? new("密码不可为空") : ValidationResult.Success
                            })
                        },
                    WebServer =
                    {
                        UrlPrefixes = new[] { Prompt.Confirm("将Http服务器开放到公网", false) ? "http://127.0.0.1:30001" : "http://+:30001" }
                    }
                };
                File.WriteAllText("setting.json", JsonConvert.SerializeObject(setting, Formatting.Indented));
                Directory.CreateDirectory("logs");
                Directory.CreateDirectory("dist");
                Logger.Warn("配置文件已生成。请重新启动");
                if (!Console.IsInputRedirected)
                {
                    Console.ReadKey(true);
                }
            }
            catch (PromptCanceledException) { }
            Runtime.ExitQuietly();
        }
    }
}