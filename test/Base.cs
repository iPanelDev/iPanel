using System;
using iPanelHost.Base;
using Xunit;

namespace iPanelHost.Tests;

public class Base
{
    [Fact]
    public void SettingsShouldThrowIfInvalid()
    {
        Assert.Throws<SettingsException>(() => new Setting().Check());

        // 子设置项判断
        Assert.Throws<SettingsException>(
            () => new Setting { InstancePassword = "6", WebServer = null!, }.Check()
        );
        Assert.Throws<SettingsException>(
            () => new Setting { InstancePassword = "6", Win32Console = null!, }.Check()
        );
        Assert.Throws<SettingsException>(
            () =>
                new Setting
                {
                    InstancePassword = "6",
                    WebServer = new() { Certificate = null! },
                }.Check()
        );

        // 目录判断
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

        // 404页面路径判断
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

        // Url列表判断
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

        // 实例密码判断
        Assert.Throws<SettingsException>(() => new Setting { InstancePassword = null!, }.Check());
    }
}
