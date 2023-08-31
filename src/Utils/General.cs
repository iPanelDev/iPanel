using System;
using System.Security.Cryptography;
using System.Text;

namespace iPanelHost.Utils
{
    public static class General
    {
        /// <summary>
        /// 获取MD5
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>MD5文本</returns>
        public static string GetMD5(string text)
            => GetMD5String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(text)));

        /// <summary>
        /// 获取MD5
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns>MD5文本</returns>
        public static string GetMD5String(byte[] targetData)
        {
            StringBuilder stringBuilder = new();
            for (int i = 0; i < targetData.Length; i++)
            {
                stringBuilder.Append(targetData[i].ToString("x2"));
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 安全读取键盘
        /// </summary>
        public static void SafeReadKey()
        {
            if (!Console.IsInputRedirected)
            {
                Console.ReadKey(true);
            }
        }
    }
}