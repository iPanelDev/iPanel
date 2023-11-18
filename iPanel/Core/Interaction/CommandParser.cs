namespace iPanel.Core.Interaction;

public abstract class CommandParser
{
    protected readonly App _app;

    protected CommandParser(App app)
    {
        _app = app;
    }

    public abstract void Parse(string[] args);
}
