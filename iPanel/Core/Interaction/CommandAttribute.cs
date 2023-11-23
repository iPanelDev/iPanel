using System;

namespace iPanel.Core.Interaction;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CommandAttribute : Attribute
{
    public CommandAttribute(string rootCommnad, string description)
    {
        RootCommand = rootCommnad;
        Description = description;
    }

    public string RootCommand { get; }

    public string Description { get; }

    public int Priority { get; init; }
}
