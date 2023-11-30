using System;
using System.Collections.Generic;

namespace iPanel.Core.Models.Users;

public class User : UserWithoutPwd
{
    public string? Password { get; set; }

    public User()
    {
        IPAddresses ??= new();
        Instances ??= Array.Empty<string>();
    }
}
