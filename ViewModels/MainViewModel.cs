
using SkyLauncher.Core.Models;
using SkyLauncher.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SkyLauncher.ViewModels;

/// <summary>
/// 全局单例 ViewModel
/// </summary>
public class MainViewModel
{

    private static MainViewModel? _instance;
    public static MainViewModel Instance => _instance ??= new MainViewModel();

    private LauncherSettings _settings;
    private List<MinecraftInstance> _instances;
    
    private MainViewModel()
    {
        _settings = LauncherSettings.Load();
        _instances = InstanceRepository.Load();

        JavaList = new ObservableCollection<JavaRuntime>();
        InstanceList = new ObservableCollection<MinecraftInstance>(_instances);

        
        AutoScanJava();
        RestoreSelectedInstance();

    }
    private bool _isManualCollocation;
    public bool IsManualCollocation
    {
        get => _isManualCollocation;
        set
        {
            if (_isManualCollocation != value)
            {
                _isManualCollocation = value;
                OnPropertyChanged(); // 通知UI更新
            }
        }
    }

    // INotifyPropertyChanged 实现
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    // ===== 数据 =====
    public ObservableCollection<JavaRuntime> JavaList { get; }
    public ObservableCollection<MinecraftInstance> InstanceList { get; }

    /// <summary>
    /// 当前选中的实例
    /// </summary>
    public MinecraftInstance? SelectedInstance {
        get => _selectedInstance;
        set
        {
            _selectedInstance = value;
            // 保存选择到设置文件
            _settings.LastSelectedInstanceJsonPath = value?.VersionJsonPath;
            _settings.Save();
        }
    }
    private MinecraftInstance? _selectedInstance;

    // ===== Java =====
    public string JavaExecutablePath
    {
        get => _settings.JavaExecutablePath;
        set { _settings.JavaExecutablePath = value; _settings.Save(); }
    }

    public string JavaVersion { get; set; } = "未检测";

    // ===== 内存 =====
    public int MinMemoryMB
    {
        get => _settings.MinMemoryMB;
        set { _settings.MinMemoryMB = value; _settings.Save(); }
    }

    public int MaxMemoryMB
    {
        get => _settings.MaxMemoryMB;
        set { _settings.MaxMemoryMB = value; _settings.Save(); }
    }

    // ===== 玩家 =====
    public string PlayerName
    {
        get => _settings.PlayerName;
        set { _settings.PlayerName = value; _settings.Save(); }
    }

    // ===== 游戏目录 =====
    public string DefaultMinecraftFolder => _settings.DefaultMinecraftFolder;

    /// <summary>
    /// 获取当前有效的 Minecraft 目录
    /// </summary>
    public string GetActualMinecraftFolder()
    {
        if (SelectedInstance != null)
            return SelectedInstance.GameDirectory;

        if (_instances.Count > 0)
            return _instances[0].GameDirectory;

        return _settings.DefaultMinecraftFolder;
    }

    /// <summary>
    /// 获取截图文件夹
    /// </summary>
    public string GetScreenshotFolder()
    {
        return Path.Combine(GetActualMinecraftFolder(), "screenshots");
    }

    // ===== 实例操作 =====
    public void AddInstance(MinecraftInstance instance)
    {
        // 去重
        var existing = _instances.FirstOrDefault(i => i.Id == instance.Id);
        if (existing != null)
            _instances.Remove(existing);

        _instances.Add(instance);
        InstanceList.Clear();
        foreach (var i in _instances)
            InstanceList.Add(i);

        SaveInstances();
    }

    public void RemoveInstance(MinecraftInstance instance)
    {
        _instances.Remove(instance);
        InstanceList.Remove(instance);
        foreach (var i in _instances)
            InstanceList.Add(i);

        // 如果删除的是当前选中的，清空
        if (SelectedInstance?.VersionJsonPath == instance.VersionJsonPath)
            SelectedInstance = null;

        SaveInstances();
    }

    public void SaveInstances()
    {
        InstanceRepository.Save(_instances);
    }

    // ===== 扫描实例 =====
    public void ScanInstances(string? minecraftFolder = null)
    {
        var folder = minecraftFolder ?? _settings.DefaultMinecraftFolder;
        if (!Directory.Exists(folder))
            return;

        var versionsDir = Path.Combine(folder, "versions");
        if (!Directory.Exists(versionsDir))
            return;

        foreach (var versionDir in Directory.GetDirectories(versionsDir))
        {
            var versionName = Path.GetFileName(versionDir);
            var jsonPath = Path.Combine(versionDir, $"{versionName}.json");

            if (!File.Exists(jsonPath))
                continue;
            if (versionName == "launcher_profiles")
                continue;

            // 检查是否已存在
            if (_instances.Any(i => i.VersionJsonPath == jsonPath))
                continue;

            // 检测版本隔离
            var isIsolated = Directory.Exists(Path.Combine(versionDir, "mods"))
                          || Directory.Exists(Path.Combine(versionDir, "config"));

            var instance = new MinecraftInstance
            {
                Name = versionName,
                RootPath = folder,
                GameDirectory = isIsolated ? versionDir : folder,
                VersionJsonPath = jsonPath,
                VersionId = versionName,
                Source = LauncherSource.SkyLauncher,
                IsManaged = true
            };

            _instances.Add(instance);
        }

        InstanceList.Clear();
        foreach (var i in _instances)
            InstanceList.Add(i);

        SaveInstances();
    }
    // 自动扫描 Java 运行时
    private void AutoScanJava()
    {
        try
        {
            var javas = JavaRuntimeService.ScanInstalledJava();
            foreach (var java in javas)
            {
                if (!JavaList.Any(j => j.ExecutablePath == java.ExecutablePath))
                    JavaList.Add(java);
            }
        }
        catch {
        Console.WriteLine("[MessageBox] 自发扫描 Java 运行时失败，请确保系统环境变量配置正确\n遇到此问题请联系zydrawer");
        }
    }

    // 恢复上次选中的实例
    private void RestoreSelectedInstance()
    {
        var lastPath = _settings.LastSelectedInstanceJsonPath;
        if (!string.IsNullOrEmpty(lastPath))
        {
            SelectedInstance = InstanceList.FirstOrDefault(i => i.VersionJsonPath == lastPath);
        }
    }

    
}