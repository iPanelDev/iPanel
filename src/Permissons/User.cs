using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace iPanelHost.Permissons
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    internal class User
    {
        /// <summary>
        /// 密码
        /// </summary>
        public string? Password;

        /// <summary>
        /// 上一次登录
        /// </summary>
        public DateTime? LastLoginTime;

        /// <summary>
        /// 权限等级
        /// </summary>
        public int Level = 0;

        /// <summary>
        /// 允许的实例
        /// </summary>
        public string[] Instances = Array.Empty<string>();

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description;

        public User()
        {
            if (Level > 3 || Level < 0)
            {
                Level = 0;
            }

            Instances ??= Array.Empty<string>();
        }

        [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public class PublicUser
        {
            public DateTime? LastLoginTime;

            public int Level = 0;

            public string[] Instances = Array.Empty<string>();

            public string? Description;

            public PublicUser(User user)
            {
                LastLoginTime = user.LastLoginTime;
                Level = user.Level;
                Instances = user.Instances;
                Description = user.Description;
            }

            public static implicit operator PublicUser(User user) => new(user);
        }
    }
}