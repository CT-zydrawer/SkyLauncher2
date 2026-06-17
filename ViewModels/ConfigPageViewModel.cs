using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Nrk.FluentCore.Environment;
using SkyLauncher.Core.Models;
using SkyLauncher.Core.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SkyLauncher.ViewModels;

public class ConfigPageViewModel : INotifyPropertyChanged
{
    public static ConfigPageViewModel? _instance;
    public static ConfigPageViewModel Instance => _instance ??= new ConfigPageViewModel();

    public ConfigPageViewModel()
    {
        AddJavaCommand = new RelayCommand(async ()=>ExecuteAddJava());
        AutoDetectJavaCommand = new RelayCommand(ExecuteAutoDetectJava);
    }

    public bool IsManualCollocation
    {
        get => MainViewModel.Instance.IsManualCollocation;
        set
        {
            if (MainViewModel.Instance.IsManualCollocation != value)
            {
                MainViewModel.Instance.IsManualCollocation = value;
                // 保存设置
                var settings = LauncherSettings.Load();
                settings.MansualCollocation = value;
                settings.Save();

                // 通知ConfigPageViewModel的属性变化
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAutoCollocation));
                OnPropertyChanged(nameof(IsManualModeEnabled));
            }
        }
    }

    public bool IsAutoCollocation
    {
        get => !IsManualCollocation;
        set
        {
            if (value)
            {
                IsManualCollocation = false;
            }
            else if (!IsManualCollocation)
            {
                IsManualCollocation = true;
            }
        }
    }

    public bool IsManualModeEnabled => IsManualCollocation;
    /*public ConfigPageViewModel()
    {
        AddJavaCommand = new RelayCommand(ExecuteAddJava);
        AutoDetectJavaCommand = new RelayCommand(ExecuteAutoDetectJava);
        if (_settings == null)
        {
            _settings = LauncherSettings.Load();
        }

    }*/

    public double MaxMemory
    {
        get => MainViewModel.Instance.MaxMemoryMB;
        set { 
            MainViewModel.Instance.MaxMemoryMB = (int)value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MinMemory));
            OnPropertyChanged(nameof(MemoryDisplayText));
        }
    }

    public double FreeMemory
    {
        get => (int)(MemoryUtils.GetWindowsMetrics().Free*0.8);
    }

    public double MinMemory
    {
        get => MainViewModel.Instance.MinMemoryMB;
        set { MainViewModel.Instance.MinMemoryMB = (int)value; OnPropertyChanged(); OnPropertyChanged(nameof(MemoryDisplayText)); }
    }

    public string MemoryDisplayText =>
        $"当前: {MainViewModel.Instance.MinMemoryMB}MB ~ {MainViewModel.Instance.MaxMemoryMB}MB ({MainViewModel.Instance.MaxMemoryMB / 1024}GB)";

    public System.Collections.ObjectModel.ObservableCollection<JavaRuntime> JavaList => MainViewModel.Instance.JavaList;

    private JavaRuntime? _selectedJava;
    public JavaRuntime? SelectedJava
    {
        get => _selectedJava;
        set
        {
            _selectedJava = value;
            if (value != null)
            {
                MainViewModel.Instance.JavaExecutablePath = value.ExecutablePath;
                MainViewModel.Instance.JavaVersion = value.Version;
            }
            OnPropertyChanged();
        }
    }

    public ICommand AddJavaCommand { get; }
    public ICommand AutoDetectJavaCommand { get; }

    private async Task ExecuteAddJava()
    {
        try
        {
            var mainWindow = ViewModelHelper.GetMainWindow();
            if (mainWindow == null)
            {
                await ViewModelHelper.ShowMessageAsync("无法获取主窗口", "错误");
                return;
            }

            // 设置文件选择器选项
            var options = new FilePickerOpenOptions
            {
                Title = "选择 Java 可执行文件",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                new FilePickerFileType("Java 运行时")
                {
                    Patterns = new[] { "javaw.exe", "java.exe" },
                    MimeTypes = new[] { "application/x-msdownload" }
                },
                new FilePickerFileType("所有文件")
                {
                    Patterns = new[] { "*" },
                    MimeTypes = new[] { "*/*" }
                }
            }
            };

            // 打开文件选择对话框
            var result = await mainWindow.StorageProvider.OpenFilePickerAsync(options);

            // 检查用户是否选择了文件
            if (result == null || result.Count == 0)
                return;

            var filePath = result[0].Path.LocalPath;

            // 导入 Java
            var java = JavaRuntimeService.ImportJava(filePath);
            if (java != null)
            {
                if (!JavaList.Any(j => j.ExecutablePath == java.ExecutablePath))
                    JavaList.Add(java);
                SelectedJava = java;
            }
        }
        catch (Exception ex)
        {
            await ViewModelHelper.ShowMessageAsync($"选择 Java 失败：{ex.Message}", "错误");
        }
    }

    private async void ExecuteAutoDetectJava()
    {
        var javaList = JavaRuntimeService.ScanInstalledJava();
        JavaList.Clear();
        foreach (var java in javaList) JavaList.Add(java);
        if (JavaList.Count > 0)
        {
            SelectedJava = JavaList[0];

            var box = MessageBoxManager.GetMessageBoxStandard(
                "信息",
                $"找到 {JavaList.Count} 个 Java",
                ButtonEnum.Ok);

            await box.ShowAsync();
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "信息",
                "未找到安装的 Java",
                ButtonEnum.Ok);

            await box.ShowAsync();
        }
    }

    public void LoadData()
    {
        // 同步设置数据
        var settings = LauncherSettings.Load();
        MainViewModel.Instance.IsManualCollocation = (bool)settings.MansualCollocation;

        // 同步 Java 选择
        var saved = MainViewModel.Instance.JavaExecutablePath;
        if (!string.IsNullOrEmpty(saved))
        {
            var existing = JavaList.FirstOrDefault(j => j.ExecutablePath == saved);
            if (existing != null) SelectedJava = existing;
        }

        // 刷新所有UI绑定
        OnPropertyChanged(nameof(IsManualCollocation));
        OnPropertyChanged(nameof(IsAutoCollocation));
        OnPropertyChanged(nameof(IsManualModeEnabled));
        OnPropertyChanged(nameof(MaxMemory));
        OnPropertyChanged(nameof(MinMemory));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}