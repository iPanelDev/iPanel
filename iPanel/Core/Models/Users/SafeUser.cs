using System;
using System.Collections.Generic;

namespace iPanel.Core.Models.Users;

public class SafeUser
{
    public DateTime? LastLoginTime { get; init; }

    public int Level { get; init; }

    public string[] Instances { get; init; } = Array.Empty<string>();

    public string? Description { get; init; }

    public List<string> IPAddresses { get; init; } = new();

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
