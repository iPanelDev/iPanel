using iPanel.Core.Models.Users;
using System;

namespace iPanel.Core.Models.Packets.Data;

public class Status
{
    public bool Logined { get; init; }

    public TimeSpan SessionDuration { get; init; }

    public UserWithoutPwd? User { get; init; }
}
