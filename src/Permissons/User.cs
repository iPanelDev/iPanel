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
        public DateTime? LastLogin;

        /// <summary>
        /// 权限等级
        /// </summary>
        public int Level = 0;

        /// <summary>
        /// 实例内容
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
        }
    }
}