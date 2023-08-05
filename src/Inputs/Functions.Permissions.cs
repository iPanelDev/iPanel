using iPanelHost.Permissons;
using iPanelHost.Utils;
using iPanelHost.WebSocket;
using iPanelHost.WebSocket.Client;
using Sharprompt;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace iPanelHost.Inputs
{
    internal static partial class Funcions
    {
        private static User _newUser => new()
        {
            Password = Prompt.Password("密码",
                placeholder: "不要与QQ或服务器等密码重复；推荐大小写字母数字结合",
                validators: new[] {
                    Validators.Required("密码不可为空"),
                    Validators.MinLength(6, "密码长度过短"),
                    Validators.RegularExpression(@"^[^\s]+$", "密码不得含有空格")
                }),
            Level = Prompt.Select<KeyValuePair<int, string>>(
                "权限等级",
                UserManager.LevelDescription,
                textSelector: (kv) => kv.Value
                ).Key,
            Description = Prompt.Input<string>("描述", placeholder: "（可选）")?.Trim()
        };

        private static string _selectUser => Prompt.Select<KeyValuePair<string, User>>(
            "选择一个用户",
            UserManager.Users,
            textSelector: (kv) => $"{kv.Key}\t权限等级: {kv.Value.Level} 上次登录时间: {kv.Value.LastLogin:d t}"
            ).Key;

        public static void ManageUsers(string[] args)
        {
            if (args.Length == 1)
            {
                Logger.Warn("缺少参数:<operation>");
                return;
            }

            switch (args[1].ToLowerInvariant())
            {
                case "a":
                case "add":
                    AddUser();
                    break;

                case "r":
                case "reload":
                    UserManager.Read();
                    Logger.Info("重新加载成功");
                    break;

                case "p":
                case "perm":
                    EditUserInstances();
                    break;

                case "d":
                case "delete":
                    DeleteUser();
                    break;

                case "ls":
                case "list":
                    lock (UserManager.Users)
                    {
                        foreach (var keyValuePair in UserManager.Users)
                        {
                            Logger.Info($"{keyValuePair.Key}\t权限等级: {keyValuePair.Value.Level} 上次登录时间: {keyValuePair.Value.LastLogin:d t}");
                        }
                    }
                    break;

                case "e":
                case "edit":
                    EditUser();
                    break;

                default:
                    Logger.Warn("参数<operation>无效");
                    break;
            }
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        private static void AddUser()
        {
            try
            {
                if (!UserManager.Add(
                    Prompt.Input<string>("帐号", validators: new[] {
                       Validators.Required("帐号不可为空"),
                       Validators.RegularExpression(@"^[^\s]+$", "帐号不得含有空格"),
                       Validators.MinLength(3, "帐号长度过短"),
                       Validators.MaxLength(16, "帐号长度过长"),
                       (value) => value is string account && !UserManager.Users.ContainsKey(account) ? ValidationResult.Success : new("帐号已存在")
                    }), _newUser))
                {
                    Logger.Warn("因字典key重复而创建失败");
                    return;
                }
                UserManager.Save();
                Logger.Info("创建成功");
            }
            catch (PromptCanceledException)
            {
                return;
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        private static void DeleteUser()
        {
            try
            {
                string key = _selectUser;

                if (key == "admin")
                {
                    Logger.Warn("此用户不可被删除");
                }

                else if (UserManager.Users.Remove(key))
                {
                    Logger.Info("删除成功");
                    UserManager.Save();
                }

                else
                {
                    Logger.Warn("因字典key变更而删除失败");
                }

            }
            catch (PromptCanceledException)
            {
                return;
            }
        }

        /// <summary>
        /// 修改用户
        /// </summary>
        private static void EditUser()
        {
            try
            {
                string key = Prompt.Select<KeyValuePair<string, User>>(
                    "选择要修改的用户",
                    UserManager.Users,
                    textSelector: (kv) => $"{kv.Key}\t权限等级: {kv.Value.Level} 上次登录时间: {kv.Value.LastLogin:d t}"
                    ).Key;
                User user = _newUser;
                lock (UserManager.Users)
                {
                    if (!UserManager.Users.ContainsKey(key))
                    {
                        Logger.Warn("因字典key丢失而修改失败");
                    }
                    else
                    {
                        UserManager.Users[key] = user;
                    }
                }
            }
            catch (PromptCanceledException)
            {
                return;
            }
            Logger.Info("修改成功");
            UserManager.Save();
        }

        /// <summary>
        /// 修改用户实例
        /// </summary>
        private static void EditUserInstances()
        {
            try
            {
                string key = _selectUser;

                if (!UserManager.Users.TryGetValue(key, out User? user))
                {
                    Logger.Warn("因字典key丢失而修改失败");
                    return;
                }

                Dictionary<string, Instance?> dict = user
                    .Instances
                    .Distinct()
                    .Select(i => new KeyValuePair<string, Instance?>(i, null))
                    .ToDictionary(kv => kv.Key, kv => kv.Value);

                lock (Handler.Instances)
                {
                    foreach (KeyValuePair<string, Instance> keyValuePair in Handler.Instances)
                    {
                        if (dict.ContainsKey(keyValuePair.Key))
                        {
                            dict[keyValuePair.Key] = keyValuePair.Value;
                        }
                    }
                }
                IEnumerable<KeyValuePair<string, Instance?>> all = dict!.Concat(Handler.Instances).Distinct()!;

                if (all.Count() == 0)
                {
                    Logger.Warn("没有在线实例且该用户的当前可使用实例为空");
                    return;
                }

                user.Instances = Prompt.MultiSelect<KeyValuePair<string, Instance?>>(
                    "选择实例",
                    all,
                    minimum: 0,
                    defaultValues: dict,
                    textSelector: (kv) => $"{kv.Key.Substring(0, 10)}({kv.Value?.Address ?? "-"}) \t自定义名称：{kv.Value?.CustomName}")
                    .Select(kv => kv.Key)
                    .ToArray();

                if (UserManager.Users.ContainsKey(key))
                {
                    UserManager.Users[key] = user;
                    UserManager.Save();
                    Logger.Info("修改权限成功");
                }
                else
                {
                    Logger.Warn("因字典key丢失而修改失败");
                    return;
                }
            }
            catch (PromptCanceledException)
            {
                return;
            }
        }
    }
}