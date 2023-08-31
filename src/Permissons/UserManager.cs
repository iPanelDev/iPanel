using iPanelHost.Utils;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Timers;

namespace iPanelHost.Permissons
{
    public static class UserManager
    {
        private static readonly Timer _timer = new(60_000) { AutoReset = true };

        static UserManager()
        {
            _timer.Elapsed += (_, _) => Save();
            _timer.Start();
        }

        public static Dictionary<string, User> Users { get; private set; } = new();

        public static Dictionary<int, string> LevelDescription = new()
        {
            { 0, "游客:   禁止登录" },
            { 1, "只读:   仅可查看" },
            { 2, "助手:   允许控制服务器" },
            { 3, "管理员: 允许控制服务器、新建修改删除用户" }
        };

        private const string _path = "users.json";

        /// <summary>
        /// 读取用户文件
        /// </summary>
        public static void Read()
        {
            if (!File.Exists(_path))
            {
                Save();
                return;
            }

            try
            {
                Users = JsonConvert.DeserializeObject<Dictionary<string, User>>(File.ReadAllText(_path)) ?? throw new FileLoadException("文件数据异常");

                if (Users.Count == 0)
                {
                    Logger.Warn("用户列表为空。请使用“user add”或“u a”添加一个用户");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal($"加载用户文件{_path}时出错");
                Logger.Fatal(e.ToString());
                General.SafeReadKey();
                Runtime.Exit();
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        public static void Save()
        {
            File.WriteAllText(_path, JsonConvert.SerializeObject(Users, Formatting.Indented));
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="account">帐号</param>
        /// <param name="user">用户对象</param>
        /// <returns>添加结果</returns>
        public static bool Add(string account, User user)
        {
            if (Users.ContainsKey(account))
            {
                return false;
            }
            Users.Add(account, user);
            Save();
            return true;
        }
    }
}