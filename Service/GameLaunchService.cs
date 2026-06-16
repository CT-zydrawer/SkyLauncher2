using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Experimental.Launch;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.GameManagement.Instances;
using SkyLauncher.Core.Models;
using SkyLauncher.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using static SkyLauncher.ViewModels.MainPageViewModel;

namespace SkyLauncher.Core.Services;

public class GameLaunchService
{
    public static Process Launch(LaunchProfile profile, MinecraftVersionInfo versionInfo)
    {
        // 1. 验证
        if (string.IsNullOrEmpty(profile.JavaExecutablePath))
            throw new InvalidOperationException("未配置 Java 运行时");
        if (!File.Exists(profile.JavaExecutablePath))
            throw new FileNotFoundException("Java 可执行文件不存在", profile.JavaExecutablePath);
        if (versionInfo == null || !File.Exists(versionInfo.JsonPath))
            throw new InvalidOperationException("无效的游戏版本");

        // 2. 解析 client.json
        var jsonPath = versionInfo.JsonPath;
        var jsonContent = File.ReadAllText(jsonPath);
        var jsonNode = JsonNode.Parse(jsonContent)
            ?? throw new InvalidOperationException("无法解析版本 JSON");
        var parentVersionId = jsonNode["inheritsFrom"]?.GetValue<string>();
        var versionId = jsonNode["id"]?.GetValue<string>()
            ?? Path.GetFileNameWithoutExtension(jsonPath);
        var versionType = jsonNode["type"]?.GetValue<string>() ?? "release";

        // 3. 创建游戏实例
        var versionFolder = Path.GetDirectoryName(jsonPath)!; // 版本文件夹路径
        var minecraftRoot = Path.GetDirectoryName(Path.GetDirectoryName(versionFolder))!; // .../.minecraft/
        string clientJarPath;
        if (!string.IsNullOrEmpty(parentVersionId))
        {
            // Fabric/Forge：jar 在原版文件夹内
            clientJarPath = Path.Combine(minecraftRoot, "versions", parentVersionId, $"{parentVersionId}.jar");
        }
        else
        {
            // 原版：jar 在自己的文件夹内
            clientJarPath = Path.Combine(versionFolder, $"{versionId}.jar");
        }

        //4. 获取 asset index ID
        var assetIndexId = jsonNode["assetIndex"]?["id"]?.GetValue<string>();
        if (string.IsNullOrEmpty(assetIndexId))
        {
            // 老版本可能没有 assetIndex，默认用 "legacy"
            assetIndexId = "legacy";
        }

        // 构建 asset index 文件路径
        var assetIndexJsonPath = Path.Combine(minecraftRoot, "assets", "indexes", $"{assetIndexId}.json");

        // MinecraftInstance 的 MinecraftFolderPath 保持为 .minecraft 根目录
        // 这样 FluentCore 才能正确找到 libraries、assets 等资源

        Nrk.FluentCore.GameManagement.Instances.MinecraftInstance instance;

        if (!string.IsNullOrEmpty(parentVersionId))
        {
            // Fabric/Forge：需要解析继承链
            var parentJsonPath = Path.Combine(minecraftRoot, "versions", parentVersionId, $"{parentVersionId}.json");
            var parentVersionFolder = Path.GetDirectoryName(parentJsonPath)!;

            var parentInstance = new VanillaMinecraftInstance
            {
                InstanceId = parentVersionId,
                MinecraftFolderPath = minecraftRoot,
                ClientJsonPath = parentJsonPath,
                ClientJarPath = Path.Combine(parentVersionFolder, $"{parentVersionId}.jar"),
                AssetIndexJsonPath = assetIndexJsonPath,
                Version = new MinecraftVersion { Type = MinecraftVersionType.Release }
            };

            instance = new ModifiedMinecraftInstance
            {
                InstanceId = versionId,
                MinecraftFolderPath = minecraftRoot,
                ClientJsonPath = jsonPath,
                ClientJarPath = Path.Combine(parentVersionFolder, $"{parentVersionId}.jar"),
                AssetIndexJsonPath = assetIndexJsonPath,
                Version = new MinecraftVersion
                {
                    Type = versionType switch
                    {
                        "release" => MinecraftVersionType.Release,
                        "snapshot" => MinecraftVersionType.Snapshot,
                        "old_beta" => MinecraftVersionType.OldBeta,
                        "old_alpha" => MinecraftVersionType.OldAlpha,
                        _ => MinecraftVersionType.Release
                    }
                },
                InheritedMinecraftInstance = parentInstance,  
                ModLoaders = new List<ModLoaderInfo>()       
            };
        }
        else
        {
            // 原版
            instance = new VanillaMinecraftInstance
            {
                InstanceId = versionId,
                MinecraftFolderPath = minecraftRoot,
                ClientJsonPath = jsonPath,
                ClientJarPath = Path.Combine(versionFolder, $"{versionId}.jar"),
                AssetIndexJsonPath = assetIndexJsonPath,
                Version = new MinecraftVersion
                {
                    Type = versionType switch
                    {
                        "release" => MinecraftVersionType.Release,
                        "snapshot" => MinecraftVersionType.Snapshot,
                        "old_beta" => MinecraftVersionType.OldBeta,
                        "old_alpha" => MinecraftVersionType.OldAlpha,
                        _ => MinecraftVersionType.Release
                    }
                }
            };
        }

        // 构建参数
        var builder = new MinecraftProcessBuilder(instance);
        builder.SetJavaSettings(profile.JavaExecutablePath, profile.MaxMemoryMB, profile.MinMemoryMB);

        if (!string.IsNullOrEmpty(versionInfo.GameDirectory))
        {
            builder.SetGameDirectory(versionInfo.GameDirectory);
        }

        // 设置离线账户
        var offlineAccount = new OfflineAccount(Name: profile.PlayerName,
Uuid: Guid.NewGuid(),
AccessToken: Guid.NewGuid().ToString("N")
);
        builder.SetAccountSettings(offlineAccount, false);

        if (Directory.Exists(Path.Combine(versionFolder, "mods"))
            || Directory.Exists(Path.Combine(versionFolder, "config")))
        {
            builder.SetGameDirectory(versionFolder);
        }

        // 添加兼容性参数
        var compatibilityArgs = new List<string>
    {
        "-XX:-OmitStackTraceInFastThrow",
        "-Djdk.lang.Process.allowAmbiguousCommands=True",
        "-Dfml.ignoreInvalidMinecraftCertificates=True",
        "-Dfml.ignorePatchDiscrepancies=True",
    };
        builder.AddVmArguments(compatibilityArgs);
        var arguments = builder.BuildArguments();
        System.Diagnostics.Debug.WriteLine("=== 生成的 JVM 参数 ===");
        foreach (var arg in arguments)
        {
            System.Diagnostics.Debug.WriteLine(arg);
        }
        System.Diagnostics.Debug.WriteLine("=== 参数结束 ===");
        // 启动
        var minecraftProcess = builder.Build();
        minecraftProcess.Start();
        return minecraftProcess.Process;
    }

    internal static object Launch(object profile, MinecraftVersionInfo versionInfo)
    {
        throw new NotImplementedException();
    }

    private static string GetAssetIndexPath(string minecraftFolder, JsonNode versionJson, string clientJsonPath)
    {
        var assetIndexNode = versionJson["assetIndex"];
        if (assetIndexNode == null)
        {
            var legacyPath = Path.Combine(minecraftFolder, "assets", "indexes", "legacy.json");
            return File.Exists(legacyPath) ? legacyPath : string.Empty;
        }

        var assetId = assetIndexNode["id"]?.GetValue<string>() ?? "legacy";
        var indexPath = Path.Combine(minecraftFolder, "assets", "indexes", $"{assetId}.json");
        return File.Exists(indexPath) ? indexPath : string.Empty;
    }
}