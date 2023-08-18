using iPanelHost.Base;
using Newtonsoft.Json;
using Sharprompt;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;

namespace iPanelHost.Utils
{
    internal static class Initialization
    {
        /// <summary>
        /// 初始化环境
        /// </summary> 
        public static void InitEnv()
        {
            // 基础
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            CrashInterception.Init();

            // 控制台
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.OutputEncoding = Encoding.UTF8;
            Console.CancelKeyPress += HandleCancelEvent;
            Win32.EnableVirtualTerminal();

            // Logger
            Swan.Logging.Logger.UnregisterLogger<Swan.Logging.ConsoleLogger>();
            Swan.Logging.Logger.RegisterLogger<Logger>();

            // Prompt输入设置
            Prompt.ThrowExceptionOnCancel = true;
            Prompt.Symbols.Done = new("√", "V");
            Prompt.ColorSchema.PromptSymbol = ConsoleColor.Blue;
            Prompt.ColorSchema.Select = ConsoleColor.DarkGray;
            Prompt.ColorSchema.Answer = ConsoleColor.Gray;
        }


        /// <summary>
        /// 上一次触发时间
        /// </summary>
        private static DateTime _lastTime;

        /// <summary>
        /// 处理Ctrl+C事件
        /// </summary>
        public static void HandleCancelEvent(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            if ((DateTime.Now - _lastTime).TotalSeconds > 1)
            {
                Logger.Warn("请在1s内再次按下`Ctrl+C`以退出。");
                _lastTime = DateTime.Now;
            }
            else
            {
                Runtime.Exit();
            }
        }

        /// <summary>
        /// 初始化设置
        /// </summary>
        public static Setting InitSetting()
        {
            try
            {
                bool toPublic = Prompt.Confirm("将Http服务器开放到公网", false);
                int port = Prompt.Input<int>(
                    "Http服务器的端口",
                    30000,
                    "1~65535",
                    new Func<object, ValidationResult?>[] {
                        (obj) => obj is int value && value > 0 && value <= 65535 ? ValidationResult.Success : new("端口无效")
                    });

                Setting setting = new()
                {
                    InstancePassword = Prompt.Password(
                        "实例连接密码",
                        placeholder: "不要与QQ或服务器等密码重复；推荐大小写字母数字结合",
                        validators: new[] {
                            Validators.Required("密码不可为空"),
                            Validators.MinLength(6, "密码长度过短"),
                            Validators.RegularExpression(@"^[^\s]+$", "密码不得含有空格"),
                        }),
                    WebServer =
                    {
                        UrlPrefixes = new[] { $"http://{(toPublic ? "+" : "127.0.0.1")}:{port}" },
                        AllowCrossOrigin = Prompt.Confirm("允许跨源资源共享（CORS）", false)
                    }
                };

                File.WriteAllText("setting.json", JsonConvert.SerializeObject(setting, Formatting.Indented));
                Directory.CreateDirectory("logs");
                Directory.CreateDirectory("dist");

                Console.WriteLine(Environment.NewLine);
                Logger.Info("初始化设置成功");

                return setting;
            }
            catch (PromptCanceledException)
            {
                Runtime.ExitQuietly();
                return null!;
            }
        }
    }
}