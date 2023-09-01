using EmbedIO;
using HttpMultipartParser;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Base.Packets.Event;
using iPanelHost.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace iPanelHost.Service;

public static class FileTransferStation
{
    public static readonly Encoding UTF8 = new UTF8Encoding(false);

    public static readonly Dictionary<string, FileItemInfo> FileItemInfos = new();

    public static readonly Timer itemsChecker = new(10000);

    static FileTransferStation()
    {
        itemsChecker.Elapsed += (_, _) => CheckFiles();
        itemsChecker.Start();
    }

    private static void CheckFiles()
    {

    }

    /// <summary>
    /// 流上传
    /// </summary>
    /// <param name="httpContext">上下文</param>
    public static async Task StreamUpload(IHttpContext httpContext)
    {
        string id = Guid.NewGuid().ToString("N");
        double time = 0;
        long byteCount = 0;
        long partByteCount = 0;
        string currentFile = string.Empty;

        DateTime start = DateTime.Now;
        Directory.CreateDirectory($"upload/{id}");
        Dictionary<string, FileStream> dict = new();
        Dictionary<string, FileItemInfo> files = new();

        Timer timer = new(2500);
        timer.Elapsed += (_, _) =>
        {
            Logger.Info($"<{httpContext.RemoteEndPoint}> 当前正在接收文件“{currentFile}” {General.GetSizeString(partByteCount / 2.5)}/s");
            partByteCount = 0;
        };
        timer.Start();

        try
        {
            StreamingMultipartFormDataParser parser = new(httpContext.Request.InputStream);
            parser.FileHandler += (name, fileName, type, disposition, buffer, bytes, partNumber, additionalProperties) =>
            {
                if (!dict.TryGetValue(fileName, out FileStream? fileStream))
                {
                    fileStream = new($"upload/{id}/{fileName}", FileMode.OpenOrCreate);
                    dict.Add(fileName, fileStream);
                }
                fileStream!.Write(buffer, 0, bytes);

                currentFile = fileName;
                byteCount += bytes;
                partByteCount += bytes;
            };

            await parser.RunAsync();
        }
        finally
        {
            foreach (var kv in dict)
            {
                files.Add(kv.Key, new(General.GetMD5String(MD5.Create().ComputeHash(kv.Value)), kv.Value.Length));
                kv.Value.Close();
            }
            timer.Stop();
        }

        time = (DateTime.Now - start).TotalSeconds;
        string speed = General.GetSizeString(byteCount / time) + "/s";

        Logger.Info($"<{httpContext.RemoteEndPoint}> 一共接收了{dict.Count}个文件，用时{time}s，平均速度{speed}");
        await httpContext.SendStringAsync(new UploadResultPacket(id, files, speed).ToString(), "text/json", UTF8);
    }

    /// <summary>
    /// 简易上传
    /// </summary>
    public static async Task SimpleUpload(IHttpContext httpContext)
    {
        string id = Guid.NewGuid().ToString("N");
        double time = 0;
        long byteCount = 0;

        Directory.CreateDirectory($"upload/{id}");
        Dictionary<string, FileItemInfo> files = new();
        MultipartFormDataParser parser = await MultipartFormDataParser.ParseAsync(httpContext.Request.InputStream);

        foreach (FilePart file in parser.Files)
        {
            DateTime start = DateTime.Now;
            Logger.Info($"<{httpContext.RemoteEndPoint}> 当前正在接收文件“{file.FileName}”({General.GetSizeString(file.Data.Length)})");

            using FileStream fileStream = new($"upload/{id}/{file.FileName}", FileMode.OpenOrCreate);
            file.Data.CopyTo(fileStream);

            double timeSpan = (DateTime.Now - start).TotalSeconds;
            Logger.Info($"<{httpContext.RemoteEndPoint}> “{file.FileName}”接收完毕，用时{timeSpan}s，平均速度{General.GetSizeString(file.Data.Length / timeSpan)}/s");


            time += timeSpan;
            byteCount += file.Data.Length;
            files.Add(file.FileName, new(General.GetMD5String(MD5.Create().ComputeHash(fileStream)), file.Data.Length));
        }
        string speed = General.GetSizeString(byteCount / time) + "/s";
        Logger.Info($"<{httpContext.RemoteEndPoint}> 一共接收了{files.Count}个文件，用时{time}s，平均速度{speed}");

        await httpContext.SendStringAsync(new UploadResultPacket(id, files, speed).ToString(), "text/json", UTF8);
    }
}
