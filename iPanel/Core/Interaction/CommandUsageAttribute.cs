using System;

namespace iPanel.Core.Interaction;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CommandUsageAttribute : Attribute
{
    public CommandUsageAttribute(string example, string description)
    {
        Example = example;
        Description = description;
    }

    public string Example { get; }

    public string Description { get; }
}
