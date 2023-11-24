using System.Reflection;

namespace iPanel;

public static class Constant
{
    public static readonly string Version = Assembly
        .GetExecutingAssembly()
        .GetName()
        .Version!.ToString();

    public const string Logo =
        @"  _ ____                  _ 
 (_)  _ \ __ _ _ __   ___| |
 | | |_) / _` | '_ \ / _ \ |
 | |  __/ (_| | | | |  __/ |
 |_|_|   \__,_|_| |_|\___|_|
 ";
}
