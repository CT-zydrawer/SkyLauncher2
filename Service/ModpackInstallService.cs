

using Nrk.FluentCore.Exceptions;
using Nrk.FluentCore.Experimental.GameManagement.Installer.Modpack;
using Nrk.FluentCore.Experimental.GameManagement.Modpacks;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Downloader;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.GameManagement.Instances;
using Nrk.FluentCore.Resources;
using Nrk.FluentCore.Resources.CurseForge;
using Nrk.FluentCore.Utils;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static Nrk.FluentCore.Resources.CurseForge.CurseForgeModpackManifest;

namespace SkyLauncher.Core.Services;

public class ModpackInstallService
{
    public event Action<string>? StatusChanged;
    public event Action<double>? ProgressChanged;

    /// <summary>
    /// 安装 CurseForge 整合包 (.zip)
    /// </summary>
    public async Task<MinecraftInstance?> InstallCurseForgeAsync(
        string filePath,
        string minecraftFolder,
        string javaPath,
        CancellationToken ct = default)
    {
        ReportStatus("正在解析整合包...");
        ReportProgress(5);

        var installer = new CurseForgeModpackInstaller
        {
            ModpackFilePath = filePath,
            MinecraftFolder = minecraftFolder,
            JavaPath = javaPath,
            CurseForgeClient = new CurseForgeClient("$2a$10$rHb3YW2DsUNaA4aVRwEwXe/w/NrWI2qJehOZF79krQoDWyJ5SReju"),
            Downloader = HttpUtils.Downloader,
            CheckAllDependencies = true,
            Progress = new Progress<IInstallerProgress>(p =>
            {

                if (p is InstallerProgress<ModrinthModpackInstaller.ModrinthModpackInstallationStage> stageProgress)
                {
                    ReportStatus(GetModrinthStageText(stageProgress.Stage, p));
                }

                if (p is DownloadTask downloadTask && downloadTask.TotalBytes.HasValue)
                {
                    var progress = (double)downloadTask.DownloadedBytes / downloadTask.TotalBytes.Value;
                    ReportProgress(10 + progress * 80.0);
                }
            })
        };

        // 监听进度

        var instance = await installer.InstallAsync(ct);

        ReportStatus("安装完成！");
        ReportProgress(100);

        return instance;
    }

    /// <summary>
    /// 安装 Modrinth 整合包 (.mrpack)
    /// </summary>
    public async Task<MinecraftInstance?> InstallModrinthAsync(
    string filePath,
    string minecraftFolder,
    string javaPath,
    CancellationToken ct = default)
    {
        ReportStatus("正在解析整合包...");
        ReportProgress(5);
        IProgress<IInstallerProgress>? progressReporter = null;
        progressReporter = new Progress<IInstallerProgress>(p =>
        {
            
            if (p is InstallerProgress<ModrinthModpackInstaller.ModrinthModpackInstallationStage> stageProgress)
            {
                ReportStatus(GetModrinthStageText(stageProgress.Stage, p));
            }

            
            if (p is DownloadTask downloadTask && downloadTask.TotalBytes.HasValue)
            {
                var progress = (double)downloadTask.DownloadedBytes / downloadTask.TotalBytes.Value;
                ReportProgress(10 + progress * 80.0);
            }
            
        });

        var installer = new ModrinthModpackInstaller
        {
            ModpackFilePath = filePath,
            MinecraftFolder = minecraftFolder,
            JavaPath = javaPath,
            //Downloader = HttpUtils.Downloader,
            CheckAllDependencies = true,
            Progress = progressReporter,
            Downloader = new MultipartDownloader(
        httpClient: HttpUtils.HttpClient,
        maxRetryCount: 5,  // 重试次数
        concurrentDownloadTasks: 3  // 并发

    ),

        };

        var instance = await Task.Run(() => installer.InstallAsync(ct));
        var instancesDir = Path.Combine(minecraftFolder, "versions");
        if (Directory.Exists(instancesDir))
        {
            foreach (var versionDir in Directory.GetDirectories(instancesDir))
            {
                var versionName = Path.GetFileName(versionDir);
                var jsonPath = Path.Combine(versionDir, $"{versionName}.json");

                if (!File.Exists(jsonPath)) continue;

                try
                {
                    var jsonContent = File.ReadAllText(jsonPath);
                    using var doc = JsonDocument.Parse(jsonContent);
                    var root = doc.RootElement;

                    var parentVersionId = root.TryGetProperty("inheritsFrom", out var inherits)
                        ? inherits.GetString()
                        : null;

                    if (!string.IsNullOrEmpty(parentVersionId))
                    {
                        var parentJsonPath = Path.Combine(instancesDir, parentVersionId, $"{parentVersionId}.json");
                        if (!File.Exists(parentJsonPath))
                        {
                            ReportStatus($"正在下载原版 {parentVersionId}...");
                            ReportProgress(50);
                            var (versionManifestItem, _) = await VersionManifestApi.SearchInstallDataAsync(parentVersionId);

                            var vanillaInstaller = new VanillaInstanceInstaller
                            {
                                MinecraftFolder = minecraftFolder,
                                McVersionManifestItem = versionManifestItem,
                                Downloader = HttpUtils.Downloader,
                                CheckAllDependencies = true
                            };

                            await vanillaInstaller.InstallAsync();
                        }
                    }
                }
                catch { ReportStatus($"解析或下载父版本失败"); ReportProgress(0); }
            }
        }
        ReportStatus("安装完成！");
        ReportProgress(100);

        return instance;
    }

    private static string GetCurseForgeStageText(
        CurseForgeModpackInstaller.CurseForgeModpackInstallationStage stage,
        IInstallerProgress progress)
    {
        return stage switch
        {
            CurseForgeModpackInstaller.CurseForgeModpackInstallationStage.ParseCurseForgeModpack => "正在解析整合包...",
            CurseForgeModpackInstaller.CurseForgeModpackInstallationStage.SearchInstallData => "正在搜索版本数据...",
            CurseForgeModpackInstaller.CurseForgeModpackInstallationStage.InstallVanillaMinecraftInstance => "正在安装原版 Minecraft...",
            CurseForgeModpackInstaller.CurseForgeModpackInstallationStage.InstallModifiedMinecraftInstance => "正在安装加载器...",
            CurseForgeModpackInstaller.CurseForgeModpackInstallationStage.ParseCurseForgeFiles => "正在解析模组列表...",
            CurseForgeModpackInstaller.CurseForgeModpackInstallationStage.DownloadCurseForgeFiles => $"正在下载模组...",
            CurseForgeModpackInstaller.CurseForgeModpackInstallationStage.CopyOverriddenFiles => "正在复制覆盖文件...",
            _ => "处理中..."
        };
    }

    private static string GetModrinthStageText(
        ModrinthModpackInstaller.ModrinthModpackInstallationStage stage,
        IInstallerProgress progress)
    {
        return stage switch
        {
            ModrinthModpackInstaller.ModrinthModpackInstallationStage.ParseModrinthModpack => "正在解析整合包...",
            ModrinthModpackInstaller.ModrinthModpackInstallationStage.SearchInstallData => "正在搜索版本数据...",
            ModrinthModpackInstaller.ModrinthModpackInstallationStage.InstallVanillaMinecraftInstance => "正在安装原版 Minecraft...",
            ModrinthModpackInstaller.ModrinthModpackInstallationStage.InstallModifiedMinecraftInstance => "正在安装加载器...",
            ModrinthModpackInstaller.ModrinthModpackInstallationStage.DownloadModrinthFiles => $"正在下载模组...",
            ModrinthModpackInstaller.ModrinthModpackInstallationStage.CopyOverriddenFiles => "正在复制覆盖文件...",
            _ => "处理中..."
        };
    }

    private void ReportStatus(string status)
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            StatusChanged?.Invoke(status);
        });
    }

    private void ReportProgress(double progress)
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            ProgressChanged?.Invoke(progress);
        });
    }
}