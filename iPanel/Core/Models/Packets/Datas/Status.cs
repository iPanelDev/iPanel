using System;

using iPanel.Core.Models.Users;

namespace iPanel.Core.Models.Packets.Data;

public class Status
{
    public bool Logined { get; init; }

    public TimeSpan SessionDuration { get; init; }

    public UserWithoutPwd? User { get; init; }
}
