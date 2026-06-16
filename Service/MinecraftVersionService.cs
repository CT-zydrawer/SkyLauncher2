using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SkyLauncher.Core.Services;

/// <summary>
/// Minecraft 版本扫描服务
/// </summary>
public static class MinecraftVersionService
{
    /// <summary>
    /// 扫描 .minecraft/versions 目录下的已安装版本
    /// </summary>
    public static List<MinecraftVersionInfo> ScanInstalledVersions(string minecraftFolder)
    {
        var versions = new List<MinecraftVersionInfo>();
        var versionsFolder = Path.Combine(minecraftFolder, "versions");

        if (!Directory.Exists(versionsFolder))
            return versions;

        foreach (var versionDir in Directory.GetDirectories(versionsFolder))
        {
            var folderName = Path.GetFileName(versionDir);
            var jsonPath = Path.Combine(versionDir, $"{folderName}.json");

            if (folderName == "launcher_profiles"||folderName == "skylauncher_config")
                continue;  //防止误将其识别为版本
            if (File.Exists(jsonPath))
            {
                try
                {
                    var info = ParseVersionInfo(jsonPath);
                    if (info != null)
                        versions.Add(info);
                }
                catch { }
            }
        }

        return versions.OrderByDescending(v => v.SortOrder).ToList();
    }

    public static MinecraftVersionInfo? ParseVersionInfo(string jsonPath)
    {
        var json = File.ReadAllText(jsonPath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var id = root.TryGetProperty("id", out var idElement) ? idElement.GetString() ?? "" : "";
        var type = root.TryGetProperty("type", out var typeElement) ? typeElement.GetString() ?? "" : "";
        var releaseTime = root.TryGetProperty("releaseTime", out var timeElement) ? timeElement.GetString() ?? "" : "";
        var mainClass = root.TryGetProperty("mainClass", out var mainClassElement) ? mainClassElement.GetString() ?? "" : "";
        var versionFolder = Path.GetDirectoryName(jsonPath);
        var isIsolated = Directory.Exists(Path.Combine(versionFolder!, "mods"))
                      || Directory.Exists(Path.Combine(versionFolder!, "config"))
                      || Directory.Exists(Path.Combine(versionFolder!, "saves"));
        // 从 inheritsFrom 判断是否为修改版（如带 Forge/Fabric 的版本）
        var inheritsFrom = root.TryGetProperty("inheritsFrom", out var inheritsElement) ? inheritsElement.GetString() : null;

        return new MinecraftVersionInfo
        {
            Id = id,
            Type = type,
            ReleaseTime = releaseTime,
            MainClass = mainClass,
            JsonPath = jsonPath,
            InheritsFrom = inheritsFrom,
            IsModified = !string.IsNullOrEmpty(inheritsFrom),
            GameDirectory = isIsolated ? versionFolder! : Path.GetDirectoryName(Path.GetDirectoryName(versionFolder!))!,
            IsIsolated = isIsolated
        };

    }
    // 在 MinecraftVersionService 类中添加

    /// <summary>
    /// 从单个 JSON 文件导入版本信息
    /// </summary>
    public static MinecraftVersionInfo? ImportVersionFromJson(string jsonPath)
    {
        if (!File.Exists(jsonPath))
            return null;

        var fileName = Path.GetFileNameWithoutExtension(jsonPath);
        if (fileName == "launcher_profiles")
            return null;

        try
        {
            var json = File.ReadAllText(jsonPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var id = root.TryGetProperty("id", out var idElement) ? idElement.GetString() ?? "" : "";

            // 如果 JSON 文件名与版本 ID 不匹配，用文件名
            if (string.IsNullOrEmpty(id))
            {
                id = Path.GetFileNameWithoutExtension(jsonPath);
            }

            var type = root.TryGetProperty("type", out var typeElement) ? typeElement.GetString() ?? "" : "";
            var releaseTime = root.TryGetProperty("releaseTime", out var timeElement) ? timeElement.GetString() ?? "" : "";
            var mainClass = root.TryGetProperty("mainClass", out var mainClassElement) ? mainClassElement.GetString() ?? "" : "";
            var inheritsFrom = root.TryGetProperty("inheritsFrom", out var inheritsElement) ? inheritsElement.GetString() : null;

            return new MinecraftVersionInfo
            {
                Id = id,
                Type = type,
                ReleaseTime = releaseTime,
                MainClass = mainClass,
                JsonPath = jsonPath,
                InheritsFrom = inheritsFrom,
                IsModified = !string.IsNullOrEmpty(inheritsFrom)
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 从版本文件夹导入（自动找文件夹内的 JSON）
    /// </summary>
    public static MinecraftVersionInfo? ImportVersionFromFolder(string versionFolderPath)
    {
        if (!Directory.Exists(versionFolderPath))
            return null;

        var folderName = Path.GetFileName(versionFolderPath);

        // 先找与文件夹同名的 JSON
        var jsonPath = Path.Combine(versionFolderPath, $"{folderName}.json");

        // 如果找不到，找任意 JSON 文件
        if (!File.Exists(jsonPath))
        {
            jsonPath = Directory.GetFiles(versionFolderPath, "*.json").FirstOrDefault() ?? "";
        }

        if (string.IsNullOrEmpty(jsonPath) || !File.Exists(jsonPath))
            return null;

        return ImportVersionFromJson(jsonPath);
    }
}

/// <summary>
/// 游戏版本信息
/// </summary>
public class MinecraftVersionInfo
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ReleaseTime { get; set; } = string.Empty;
    public string MainClass { get; set; } = string.Empty;
    public string JsonPath { get; set; } = string.Empty;
    public string? InheritsFrom { get; set; }
    public bool IsModified { get; set; }
    public string GameDirectory { get; set; } = string.Empty;
    public bool IsIsolated { get; set; } = false;
    public string DisplayText => IsModified ? $"{Id} (基于 {InheritsFrom})" : Id;
    public string ScreenshotFolder => Path.Combine(GameDirectory, "screenshots");
    /// <summary>
    /// 排序优先级：正式版 > 快照 > 远古版本
    /// </summary>
    public int SortOrder => Type switch
    {
        "release" => 100,
        "snapshot" => 50,
        _ => 0
    };

}
