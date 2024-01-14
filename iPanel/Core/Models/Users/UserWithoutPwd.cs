using System;
using System.Collections.Generic;

namespace iPanel.Core.Models.Users;

public class UserWithoutPwd
{
    public DateTime? LastLoginTime { get; set; }

    public PermissionLevel Level { get; set; }

    public string[] Instances { get; set; } = Array.Empty<string>();

    public string Description { get; set; } = string.Empty;

    public List<string> IPAddresses { get; set; } = new();
}
