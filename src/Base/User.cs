using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace iPanelHost.Base;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class User
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
    public PermissonLevel Level = 0;

    /// <summary>
    /// 允许的实例
    /// </summary>
    public string[] Instances = Array.Empty<string>();

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description;

    public List<string> IPAddresses = new();

    public User()
    {
        IPAddresses ??= new();
        Instances ??= Array.Empty<string>();
    }
}