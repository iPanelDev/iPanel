using Newtonsoft.Json;
using iPanel.Base;
using iPanel.Core.Service;
using iPanel.Utils;
using System;
using System.IO;

namespace iPanel
{
    internal static class Program
    {
        /// <summary>
        /// 版本
        /// </summary>
        public const string VERSION = "2.0";

        /// <summary>
        /// 设置
        /// </summary>
        public static Setting? Setting { get; private set; }

        /// <summary>
        /// 应用程序的主入口点
        /// </summary>
        /// <param name="args">启动参数</param>
        [STAThread]
        private static void Main(string[] args)
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            RunTime.SetConsole();
            CrashInterception.Init();
            if (!File.Exists("setting.json"))
            {
                File.WriteAllText("setting.json", JsonConvert.SerializeObject(new Setting(), Formatting.Indented));
                throw new SettingsException("配置文件已生成，请修改后重新启动");
            }
            try
            {
                Setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText("setting.json")) ?? throw new SettingsException("转换出现异常空值");
                if (string.IsNullOrEmpty(Setting.Password))
                {
                    throw new SettingsException("密码不可为空，请修改后重试。");
                }
            }
            catch (Exception e)
            {
                throw new LocalException("读取文件时出现错误", e);
            }
            Connections.Start();
            RunTime.StartHandleInput();
        }
    }
}
