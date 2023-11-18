using iPanel.Core.Models.Packets.Event;

namespace iPanel.Core.Models.Packets.Data;

public class Result
{
    public bool? Success { get; init; }

    public int? Code { get; init; }

    public string? Reason { get; init; }

    public Result(string? reason)
    {
        Reason = reason;
        Success = null;
    }

    public Result(string? reason, bool success)
    {
        Reason = reason;
        Success = success;
    }

    public Result(ResultTypes resultTypes)
    {
        Reason = resultTypes.ToString();
        Code = (int)resultTypes;
        Success = resultTypes == ResultTypes.Success;
    }
}
