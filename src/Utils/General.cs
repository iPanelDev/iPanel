using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace iPanelHost.Utils;

public static class General
{
    /// <summary>
    /// 获取MD5
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>MD5文本</returns>
    public static string GetMD5(string text) =>
        GetHexString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(text)));

    /// <summary>
    /// 获取MD5
    /// </summary>
    /// <param name="bytes">字节数组</param>
    /// <returns>MD5文本</returns>
    public static string GetMD5(byte[] bytes) => GetHexString(MD5.Create().ComputeHash(bytes));

    /// <summary>
    /// 获取MD5
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns>MD5文本</returns>
    public static string GetMD5(Stream stream) => GetHexString(MD5.Create().ComputeHash(stream));

    /// <summary>
    /// 获取MD5
    /// </summary>
    /// <param name="bytes">字节数组</param>
    /// <returns>MD5文本</returns>
    public static string GetHexString(byte[] targetData)
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

    /// <summary>
    /// 获取大小的文本
    /// </summary>
    public static string GetSizeString(double size)
    {
        if (size < 1024)
        {
            return $"{size}B";
        }
        double _size = size;
        if (_size < 1024 * 1024)
        {
            return $"{size / 1024:N2}KB";
        }
        if (_size < 1024 * 1024 * 1024)
        {
            return $"{size / 1024 / 1024:N2}MB";
        }
        return $"{size / 1024 / 1024 / 1024:N2}GB";
    }
}
