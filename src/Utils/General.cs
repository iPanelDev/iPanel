using System.Security.Cryptography;
using System.Text;

namespace iPanelHost.Utils
{
    internal static class General
    {
        /// <summary>
        /// 获取MD5
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>MD5文本</returns>
        public static string GetMD5(string text)
        {
            byte[] targetData = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(text));
            string result = string.Empty;
            for (int i = 0; i < targetData.Length; i++)
            {
                result += targetData[i].ToString("x2");
            }
            return result;
        }
    }
}