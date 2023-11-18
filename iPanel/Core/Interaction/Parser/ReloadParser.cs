namespace iPanel.Core.Interaction.Parser;

[CommandParser("reload", "读取设置并重新启动服务器")]
public class ReloadParser : CommandParser
{
    public ReloadParser(App app)
        : base(app) { }

    public override void Parse(string[] args)
    {
        _app.Reload();
    }
}
