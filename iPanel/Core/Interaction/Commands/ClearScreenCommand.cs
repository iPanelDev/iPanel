using Microsoft.Extensions.Hosting;
using System;

namespace iPanel.Core.Interaction.Commands;

[CommandDescription("cls", "清屏")]
public class ClearScreenCommand : Command
{
    public ClearScreenCommand(IHost host)
        : base(host) { }

    public override void Parse(string[] args)
    {
        Console.Clear();
    }
}
