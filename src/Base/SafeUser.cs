using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class SafeUser
{
    public readonly DateTime? LastLoginTime;

    public readonly int Level;

    public readonly string[] Instances = Array.Empty<string>();

    public readonly string? Description;

    public readonly List<string> IPAddresses = new();

    public SafeUser(User user)
    {
        LastLoginTime = user.LastLoginTime;
        Level = (int)user.Level;
        Instances = user.Instances;
        Description = user.Description;
        IPAddresses = user.IPAddresses;
    }

    public static implicit operator SafeUser(User user) => new(user);
}
