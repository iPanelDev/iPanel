using EmbedIO;
using HttpMultipartParser;
using iPanelHost.Base.Packets.DataBody;
using iPanelHost.Server;
using iPanelHost.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace iPanelHost.Service;

public static class FileTransferStation
{
    public static readonly Encoding UTF8 = new UTF8Encoding(false);

    public static readonly Dictionary<string, FileItemInfo> FileItemInfos = new();

    public static readonly Timer _checker = new(10000);

    static FileTransferStation()
    {
        _checker.Elapsed += (_, _) => CheckFiles();
        _checker.Start();
        if (!File.Exists("fileInfos.json"))
        {
            return;
        }
        try
        {
            FileItemInfos =
                JsonConvert.DeserializeObject<Dictionary<string, FileItemInfo>>(
                    File.ReadAllText("fileInfos.json")
                ) ?? new();
        }
        catch (Exception e)
        {
            Logger.Error($"fileInfos.json”时出现问题: {e.Message}");
        }
    }

    /// <summary>
    /// 检查文件列表
    /// </summary>
    private static void CheckFiles()
    {
        foreach (var keyValuePair in FileItemInfos.ToArray())
        {
            if (!File.Exists(keyValuePair.Key))
            {
                FileItemInfos.Remove(keyValuePair.Key);
                continue;
            }
            if (keyValuePair.Value.Expires < DateTime.Now)
            {
                File.Delete(keyValuePair.Key);
                Logger.Warn($"上传的文件“{keyValuePair.Key}”过期，已被删除");
                FileItemInfos.Remove(keyValuePair.Key);
                continue;
            }
        }
        File.WriteAllText("fileInfos.json", JsonConvert.SerializeObject(FileItemInfos, Formatting.Indented));
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
            Logger.Info(
                $"<{httpContext.RemoteEndPoint}> 当前正在接收文件“{currentFile}” {General.GetSizeString(partByteCount / 2.5)}/s"
            );
            partByteCount = 0;
        };
        timer.Start();

        try
        {
            StreamingMultipartFormDataParser parser = new(httpContext.Request.InputStream);
            parser.FileHandler += (_, fileName, _, _, buffer, bytes, partNumber, _) =>
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return;
                }
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
                files.Add(
                    kv.Key,
                    new(General.GetMD5String(MD5.Create().ComputeHash(kv.Value)), kv.Value.Length)
                );
                kv.Value.Close();
            }
            timer.Stop();
        }

        foreach (var kv in files)
        {
            FileItemInfos.Add(kv.Key, kv.Value);
        }

        time = (DateTime.Now - start).TotalSeconds;
        string speed = General.GetSizeString(byteCount / time) + "/s";

        Logger.Info($"<{httpContext.RemoteEndPoint}> 一共接收了{dict.Count}个文件，用时{time}s，平均速度{speed}");
        await Apis.SendJson(httpContext, new UploadResult() { ID = id, Files = files, Speed = speed }, true);
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
        MultipartFormDataParser parser = await MultipartFormDataParser.ParseAsync(
            httpContext.Request.InputStream
        );

        foreach (FilePart file in parser.Files)
        {
            DateTime start = DateTime.Now;
            Logger.Info(
                $"<{httpContext.RemoteEndPoint}> 当前正在接收文件“{file.FileName}”({General.GetSizeString(file.Data.Length)})"
            );

            if (string.IsNullOrEmpty(file.FileName))
            {
                return;
            }
            using FileStream fileStream =
                new($"upload/{id}/{file.FileName}", FileMode.OpenOrCreate);
            file.Data.CopyTo(fileStream);

            double timeSpan = (DateTime.Now - start).TotalSeconds;
            Logger.Info(
                $"<{httpContext.RemoteEndPoint}> “{file.FileName}”接收完毕，用时{timeSpan}s，平均速度{General.GetSizeString(file.Data.Length / timeSpan)}/s"
            );

            time += timeSpan;
            byteCount += file.Data.Length;
            files.Add(
                file.FileName,
                new(General.GetMD5String(MD5.Create().ComputeHash(fileStream)), file.Data.Length)
            );
        }

        foreach (var kv in files)
        {
            FileItemInfos.Add(kv.Key, kv.Value);
        }

        string speed = General.GetSizeString(byteCount / time) + "/s";
        Logger.Info($"<{httpContext.RemoteEndPoint}> 一共接收了{files.Count}个文件，用时{time}s，平均速度{speed}");

        await Apis.SendJson(httpContext, new UploadResult() { ID = id, Files = files, Speed = speed }, true);
    }
}
