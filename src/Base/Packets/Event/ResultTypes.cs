namespace iPanelHost.Base.Packets.Event;

public enum ResultTypes
{
    Unknown = -1,
    Success,
    DataAnomaly,
    DuplicateInstanceID,
    EmptyAccount,
    ErrorWhenGettingPacketContent,
    FailToVerify,
    IncorrectAccountOrPassword,
    IncorrectClientType,
    IncorrectInstanceID,
    InternalDataError,
    InvalidArgs,
    InvalidConsole,
    InvalidTarget,
    InvalidUser,
    LostArgs,
    NotVerifyYet,
    PermissionDenied,
    TimeoutInVerification,
}
