using iPanelHost.Utils;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;

namespace iPanelHost.Permissons
{
    internal static class UserManager
    {
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

                if (!Users.ContainsKey("admin"))
                {
                    Add("admin", new()
                    {
                        Level = 3
                    });
                    Logger.Warn("最高控制权限用户“admin”不存在，现已重新创建。");
                    Save();
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