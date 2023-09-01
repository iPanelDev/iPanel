namespace iPanelHost.Base.Packets.Event;

public enum ResultTypes
{
    Unknown = -1,
    None,
    DataAnomaly,
    DuplicateInstanceID,
    EmptyAccount,
    ErrorWhenGettingPacketContent,
    FailToVerify,
    IncorrectAccountOrPassword,
    IncorrectClientType,
    IncorrectInstanceID,
    InternalDataError,
    InvalidConsole,
    InvalidTarget,
    InvalidUser,
    NotVerifyYet,
    PermissionDenied,
    TimeoutInVerification,
}
