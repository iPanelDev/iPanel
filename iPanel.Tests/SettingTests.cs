using System;

using iPanel.Core.Models.Exceptions;
using iPanel.Core.Models.Settings;

using Xunit;

namespace iPanel.Tests;

public class SettingTests
{
    [Fact]
    public void SettingsShouldThrowIfInvalid()
    {
        Assert.Throws<SettingsException>(() => new Setting().Check());

        Assert.Throws<SettingsException>(
            () => new Setting { InstancePassword = "6", WebServer = null!, }.Check()
        );

        Assert.Throws<SettingsException>(
            () =>
                new Setting
                {
                    InstancePassword = "6",
                    WebServer = new() { Certificate = null! },
                }.Check()
        );

        Assert.Throws<SettingsException>(
            () =>
                new Setting
                {
                    InstancePassword = "6",
                    WebServer = new() { Directory = string.Empty },
                }.Check()
        );
        Assert.Throws<SettingsException>(
            () =>
                new Setting
                {
                    InstancePassword = "6",
                    WebServer = new() { Directory = null! },
                }.Check()
        );

        Assert.Throws<SettingsException>(
            () =>
                new Setting
                {
                    InstancePassword = "6",
                    WebServer = new() { Page404 = string.Empty },
                }.Check()
        );
        Assert.Throws<SettingsException>(
            () =>
                new Setting
                {
                    InstancePassword = "6",
                    WebServer = new() { Page404 = null! },
                }.Check()
        );

        Assert.Throws<SettingsException>(
            () =>
                new Setting
                {
                    InstancePassword = "6",
                    WebServer = new() { UrlPrefixes = Array.Empty<string>() },
                }.Check()
        );
        Assert.Throws<SettingsException>(
            () =>
                new Setting
                {
                    InstancePassword = "6",
                    WebServer = new() { UrlPrefixes = null! },
                }.Check()
        );

        Assert.Throws<SettingsException>(() => new Setting { InstancePassword = null!, }.Check());

        Assert.Throws<SettingsException>(
            () =>
                new Setting
                {
                    InstancePassword = "6",
                    WebServer = new() { MaxRequestsPerSecond = -1 }
                }.Check()
        );
    }
}
