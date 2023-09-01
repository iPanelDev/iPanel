using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace iPanelHost.Base;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class SafeUser
{
    public DateTime? LastLoginTime;

    public int Level = 0;

    public string[] Instances = Array.Empty<string>();

    public string? Description;

    public List<string> IPAddresses = new();

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