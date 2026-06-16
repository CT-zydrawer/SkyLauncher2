using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Nrk.FluentCore.Environment;
using SkyLauncher.Core.Models;
using SkyLauncher.Core.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SkyLauncher.ViewModels;

public class ConfigPageViewModel : INotifyPropertyChanged
{
    public static ConfigPageViewModel? _instance;
    public static ConfigPageViewModel Instance => _instance ??= new ConfigPageViewModel();

    public ConfigPageViewModel()
    {
        AddJavaCommand = new RelayCommand(ExecuteAddJava);
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

    private async void ExecuteAddJava()
    {
        
    }
 
    private void ExecuteAutoDetectJava()
    {
        var javaList = JavaRuntimeService.ScanInstalledJava();
        JavaList.Clear();
        foreach (var java in javaList) JavaList.Add(java);
        if (JavaList.Count > 0) SelectedJava = JavaList[0];
        Console.WriteLine($"[MessageBox] 未找到已安装的 Java");
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