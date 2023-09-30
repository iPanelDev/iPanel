namespace iPanelHost.Base.Packets.Event;

public enum ResultTypes
{
    Unknown = -1,
    Success,
    DataAnomaly,
    DuplicateInstanceID,
    EmptyUserName,
    ErrorWhenGettingPacketContent,
    FailToVerify,
    IncorrectUserNameOrPassword,
    IncorrectClientType,
    IncorrectInstanceID,
    InternalDataError,
    InvalidArgs,
    InvalidConsole,
    InvalidSubscription,
    InvalidUser,
    LostArgs,
    NotVerifyYet,
    PermissionDenied,
    TimeoutInVerification,
    Timeout
}
