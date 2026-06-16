
using Avalonia;
using Avalonia.Input;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SkyLauncher.Core.Models;    
using SkyLauncher.Core.Services;
using SkyLauncher.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SkyLauncher.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
    private bool _isUploading;
    public class LaunchProfile
    {
        public string JavaExecutablePath { get; set; }

        public int MinMemoryMB { get; set; }

        public int MaxMemoryMB { get; set; }

        public string PlayerName { get; set; }

        public string MinecraftFolder { get; set; }
    }
    public MainPageViewModel()
    {
        LaunchCommand = new RelayCommand(ExecuteLaunch);
    }

    public bool IsUploading
    {
        get => _isUploading;
        set { _isUploading = value; OnPropertyChanged(); }
    }

    public string PlayerName
    {
        get => MainViewModel.Instance.PlayerName;
        set { MainViewModel.Instance.PlayerName = value; OnPropertyChanged(); }
    }

    public string JavaDisplayInfo
    {
        get
        {
            var path = MainViewModel.Instance.JavaExecutablePath;
            var version = MainViewModel.Instance.JavaVersion;
            return string.IsNullOrEmpty(path) ? "未配置 Java" : $"Java {version}";
        }
    }

    public string SelectedVersionInfo
    {
        get
        {
            var inst = MainViewModel.Instance.SelectedInstance;
            if (inst == null) return "未选择版本";
            return $"{inst.Name}";
        }
    }

    public bool CanLaunch =>
        !string.IsNullOrEmpty(MainViewModel.Instance.JavaExecutablePath)
        && !string.IsNullOrEmpty(MainViewModel.Instance.PlayerName)
        && MainViewModel.Instance.SelectedInstance != null;

    public ICommand LaunchCommand { get; }

    private async void ExecuteLaunch()
    {
        if (!CanLaunch)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("标题", "消息内容", ButtonEnum.Ok);
            await box.ShowAsync();
            IsUploading = false;
            return;
        }

        try
        {
            IsUploading = true;
            var inst = MainViewModel.Instance.SelectedInstance!;

            var profile = new LaunchProfile
            {
                JavaExecutablePath = MainViewModel.Instance.JavaExecutablePath,
                MaxMemoryMB = MainViewModel.Instance.MaxMemoryMB,
                MinMemoryMB = MainViewModel.Instance.MinMemoryMB,
                PlayerName = MainViewModel.Instance.PlayerName,
                MinecraftFolder = inst.GameDirectory
            };

            var versionInfo =
    MinecraftVersionService.ParseVersionInfo(
        inst.VersionJsonPath);

            if (versionInfo == null)
            {
                throw new Exception(
                    "无法解析游戏版本 JSON");
            }

            var process = GameLaunchService.Launch(profile, versionInfo);

            if (process != null)
            {
                /* TODO: Get main window reference */
                // null.WindowState = WindowState.Minimized;
                process.Exited += (s, e) =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        IsUploading = false;
                        /* TODO: Get main window reference */
                        // null.WindowState = WindowState.Normal;
                    });
                };
                process.EnableRaisingEvents = true;
            }
        }
        catch (Exception ex)
        {
            IsUploading = false;
            var box = MessageBoxManager.GetMessageBoxStandard("错误!", "启动失败", ButtonEnum.Ok);
            await box.ShowAsync();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}