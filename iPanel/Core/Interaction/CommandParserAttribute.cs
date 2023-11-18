using System;

namespace iPanel.Core.Interaction;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CommandParserAttribute : Attribute
{
    public CommandParserAttribute(string rootCommnad, string description)
    {
        RootCommand = rootCommnad;
        Description = description;
    }

    public string RootCommand { get; }

    public string Description { get; }

    public string? Alias { get; init; }

    public int Priority { get; init; }
}
