using System;

namespace iPanelHost
{
    internal static class Constant
    {
        /// <summary>
        /// 版本
        /// </summary>
        public static readonly string VERSION = new Version(2, 2, 0).ToString();

        /// <summary>
        /// 内部版本
        /// </summary>
        public const int InternalVersion = 1;

        public const string Logo = @"
  _ ____                  _   _   _           _   
 (_)  _ \ __ _ _ __   ___| | | | | | ___  ___| |_ 
 | | |_) / _` | '_ \ / _ \ | | |_| |/ _ \/ __| __|
 | |  __/ (_| | | | |  __/ | |  _  | (_) \__ \ |_ 
 |_|_|   \__,_|_| |_|\___|_| |_| |_|\___/|___/\__|
 ";

        public const string LogoIco = @"
\x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;114;152;181m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;114;152;181m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;114;152;181m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[0m  \x1b[48;2;114;152;181m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[48;2;114;152;181m  \x1b[48;2;114;152;181m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[48;2;71;105;133m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[48;2;114;152;181m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;114;152;181m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;114;152;181m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;114;152;181m  \x1b[48;2;90;120;142m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;101;135;160m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;101;135;160m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;101;135;160m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;101;135;160m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;101;135;160m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;101;135;160m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[0m  \x1b[48;2;114;152;181m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;101;135;160m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;114;152;181m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;101;135;160m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;114;152;181m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;101;135;160m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;114;152;181m  \x1b[48;2;114;152;181m  \x1b[0m  \x1b[48;2;101;135;160m  \x1b[0m  \x1b[48;2;71;105;133m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m
\x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[48;2;114;152;181m  \x1b[48;2;90;120;142m  \x1b[48;2;71;105;133m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m  \x1b[0m
           
  _ ____                  _   _   _           _   
 (_)  _ \ __ _ _ __   ___| | | | | | ___  ___| |_ 
 | | |_) / _` | '_ \ / _ \ | | |_| |/ _ \/ __| __|
 | |  __/ (_| | | | |  __/ | |  _  | (_) \__ \ |_ 
 |_|_|   \__,_|_| |_|\___|_| |_| |_|\___/|___/\__|
";
    }
}