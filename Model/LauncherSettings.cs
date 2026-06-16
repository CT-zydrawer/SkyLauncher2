
using Nrk.FluentCore.Environment;
using System;
using System.IO;
using System.Text.Json;

namespace SkyLauncher.Core.Models;

public class LauncherSettings
{
    private static readonly string ConfigFolder =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "SkyLauncher");

    private static readonly string ConfigPath =
        Path.Combine(ConfigFolder, "settings.json");

    // ===== Java&&Memory =====

    public string JavaExecutablePath { get; set; } = string.Empty;

    public int MinMemoryMB { get; set; } = 1024;

    public int MaxMemoryMB { get; set; } = (int)((MemoryUtils.GetWindowsMetrics().Free)*0.8);

    public bool MansualCollocation { get; set; } = true;

    // ===== User =====

    public string PlayerName { get; set; } = "Steve";

    // ===== Login =====

    public bool IsOnlineMode { get; set; } = false;//暂时空，搁置

    // ===== Minecraft =====

    public string DefaultMinecraftFolder { get; set; } =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            ".minecraft");

    // ===== UI =====

    public bool AutoScanInstances { get; set; } = true;

    public bool ShowSnapshots { get; set; } = true;

    public bool DarkModeEnable { get; set; } = false ;//由于HandyControl导致工作量较大，搁置

    //=====Personiziation=====

    public string ThemeColorSetting { get; set; } = "";

    public string PictureBackgroundPath { get; set; } = "";

    public float LauncherOpacity { get; set; } = 0.85f;

    // ===== Load / Save =====

    public static LauncherSettings Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);

                return JsonSerializer.Deserialize<LauncherSettings>(json)
                    ?? new LauncherSettings();
            }
        }
        catch
        {
            Console.WriteLine($"[MessageBox] 无法加载设置，缺省的设置将会被使用\n遇到此问题请联系zydrawer");
        }

        return new LauncherSettings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(ConfigFolder);

            var json = JsonSerializer.Serialize(
                this,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(ConfigPath, json);
        }
        catch
        {
            Console.WriteLine($"[MessageBox] 保存失败，下次启动时将会使用缺省设置");
        }
    }

    public string? LastSelectedInstanceJsonPath { get; set; }
}