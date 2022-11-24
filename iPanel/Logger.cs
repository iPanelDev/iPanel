using System;

namespace iPanel
{
    internal static class Logger
    {
        public static void Info(string Line)
            => Console.WriteLine("\x1b[96m[Info]\x1b[0m" + Line);

        public static void Warn(string Line)
            => Console.WriteLine($"\x1b[33m[Warn]{Line}\x1b[0m");

        public static void Error(string Line)
            => Console.WriteLine($"\x1b[91m[Error]{Line}\x1b[0m");

        public static void Normal(string Line)
            => Console.WriteLine(Line);

        public static void Connect(string Line)
            => Console.WriteLine($"\x1b[36m[＋]\x1b[0m{Line}");

        public static void Disconnect(string Line)
            => Console.WriteLine($"\x1b[36m[－]\x1b[0m{Line}");

        public static void Notice(string Line)
            => Console.WriteLine($"\x1b[93m[﹗]\x1b[0m{Line}");

        public static void Recieve(string Line)
            => Console.WriteLine($"\x1b[92m[↓]\x1b[0m{Line}");

        public static void Send(string Line)
            => Console.WriteLine($"\x1b[96m[↑]\x1b[0m{Line}");
    }
}
