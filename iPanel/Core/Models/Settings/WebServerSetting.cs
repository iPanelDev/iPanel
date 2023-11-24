using System;

namespace iPanel.Core.Models.Settings;

public class WebServerSetting
{
    public string[] UrlPrefixes { get; init; } = { "http://127.0.0.1:30000" };

    public string Directory { get; init; } = "dist";

    public bool DisableFilesHotUpdate { get; init; } = true;

    public string Page404 { get; init; } = "index.html";

    public bool AllowCrossOrigin { get; init; }

    public int MaxRequestsPerSecond { get; init; } = 50;

    public string[] WhiteList { get; init; } = Array.Empty<string>();

    public CertificateSettings Certificate { get; init; } = new();
}
