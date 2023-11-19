namespace iPanel.Core.Interaction.Parser;

[Command("exit", "关闭并退出", Priority = int.MinValue)]
public class ExitParser : CommandParser
{
    public ExitParser(App app)
        : base(app) { }

    public override void Parse(string[] args)
    {
        _app.Dispose();
    }
}
