using System;
using System.Collections.Generic;

namespace iPanel.Core.Models.Users;

public class User
{
    public string? Password { get; set; }

    public DateTime? LastLoginTime { get; set; }

    public PermissionLevel Level { get; set; }

    public string[] Instances { get; set; } = Array.Empty<string>();

    public string? Description { get; set; }

    public List<string> IPAddresses { get; set; } = new();

    public User()
    {
        IPAddresses ??= new();
        Instances ??= Array.Empty<string>();
    }
}
