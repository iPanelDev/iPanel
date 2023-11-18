using System;
using System.Linq;

namespace iPanel.Core.Models.Packets.Data;

public class DirInfo
{
    public FileItem[] Items { get; init; }

    public bool IsExist { get; init; }

    public string? Path { get; init; }

    public DirInfo()
    {
        Items =
            Items?.Where((item) => item?.Type == "file" || item?.Type == "dir").ToArray()
            ?? Array.Empty<FileItem>();
    }

    public class FileItem
    {
        public string? Type { get; init; }

        public string? Path { get; init; }

        public string? Name { get; init; }

        public long? Size { get; init; }
    }
}
