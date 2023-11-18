using System;

namespace iPanel.Core.Interaction.Parser;

[CommandParser("cls", "清屏", Priority = -3)]
public class ClearScreenParser : CommandParser
{
    public ClearScreenParser(App app)
        : base(app) { }

    public override void Parse(string[] args)
    {
        Console.Clear();
    }
}
