namespace iPanel.Base
{
    internal class Setting
    {
        public string Addr { get; set; } = "ws://0.0.0.0:30000";
        public string Password { get; set; } = string.Empty;
        public bool Debug { get; set; }
    }
}
