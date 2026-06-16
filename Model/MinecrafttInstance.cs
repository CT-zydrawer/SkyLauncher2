using System;
using System.IO;

namespace SkyLauncher.Core.Models;

public class MinecraftInstance
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    // 实例根目录
    public string RootPath { get; set; } = string.Empty;

    // 真正传给 --gameDir 的目录
    public string GameDirectory { get; set; } = string.Empty;

    // version json
    public string VersionJsonPath { get; set; } = string.Empty;

    // Minecraft Version
    public string VersionId { get; set; } = string.Empty;

    // Forge/Fabric/NeoForge...
    public string? LoaderName { get; set; }

    // 来源
    public LauncherSource Source { get; set; }

    // 是否由本启动器管理
    public bool IsManaged { get; set; }

    // 路径索引（非常重要）
    public string ModsPath =>
        Path.Combine(GameDirectory, "mods");

    public string SavesPath =>
        Path.Combine(GameDirectory, "saves");

    public string ConfigPath =>
        Path.Combine(GameDirectory, "config");

    public string ResourcePacksPath =>
        Path.Combine(GameDirectory, "resourcepacks");

}