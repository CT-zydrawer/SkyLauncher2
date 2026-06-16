using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using SkyLauncher.Core.Models;
using SkyLauncher.Views;

namespace SkyLauncher.ViewModels;

public class VersionManagementViewModel : INotifyPropertyChanged
{
    private static VersionManagementViewModel? _instance;
    public static VersionManagementViewModel Instance => _instance ??= new VersionManagementViewModel();

    public VersionManagementViewModel()
    {
        RefreshCommand = new RelayCommand(ExecuteRefresh);
        ImportVersionCommand = new RelayCommand(ExecuteImportVersion);
        ImportFolderCommand = new RelayCommand(ExecuteImportFolder);
        DeleteInstanceCommand = new RelayCommand<SkyLauncher.Core.Models.MinecraftInstance>(ExecuteDeleteInstance);
    }

    public ObservableCollection<SkyLauncher.Core.Models.MinecraftInstance> InstanceList => MainViewModel.Instance.InstanceList;

    private SkyLauncher.Core.Models.MinecraftInstance? _selectedInstance;
    public SkyLauncher.Core.Models.MinecraftInstance? SelectedInstance
    {
        get => _selectedInstance;
        set
        {
            _selectedInstance = value;
            if (value != null) MainViewModel.Instance.SelectedInstance = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedInstanceInfo));
        }
    }

    private void ExecuteDeleteInstance(SkyLauncher.Core.Models.MinecraftInstance? instance)
    {
        if (instance == null) return;

        Console.WriteLine($"[MessageBox] 确定要删除实例 \"{instance.Name}\" 吗？此操作不会删除游戏文件。");

        // TODO: Check dialog result
        if (true) {
            MainViewModel.Instance.RemoveInstance(instance);

            // 如果删除的是当前选中的，清空选择
            if (SelectedInstance == instance)
                SelectedInstance = null;

            OnPropertyChanged(nameof(InstanceList));
        }
    }
    public string SelectedInstanceInfo
    {
        get
        {
            //var deleteButton = (TopLevel.GetTopLevel(this) as Window)?.FindName("DeleteInstanceButton") as Button;
            if (SelectedInstance == null) return "未选择实例";
            var inst = SelectedInstance;
            return $"实例：{inst.Name}\n版本id：{inst.VersionId}\n文件路径： {inst.GameDirectory}\n{(inst.LoaderName ?? "原版")}";
            //deleteButton.SetValue(UIElement.VisibilityProperty, Visibility.Visible);
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand ImportVersionCommand { get; }
    public ICommand ImportFolderCommand { get; }
    public ICommand DeleteInstanceCommand { get; }

    private void ExecuteRefresh()
    {
        MainViewModel.Instance.ScanInstances();
        OnPropertyChanged(nameof(InstanceList));
    }

    private void ExecuteImportVersion()
    {
        // TODO: OpenFileDialog - use StorageProvider.OpenFilePickerAsync
        var dialog = new FilePickerOpenOptions { Title = "选择文件", AllowMultiple = false };

        // var mainWindow = (TopLevel.GetTopLevel(this) as Window);
        // bool? result = await StorageProvider.OpenFilePickerAsync(dialog);

        // TODO: Check dialog result
        if (true) {
            ImportVersionFromJson("placeholder");
        }
    }

    private void ExecuteImportFolder()
    {
        // TODO: OpenFolderDialog - use StorageProvider.OpenFolderPickerAsync
        var dialog = new FolderPickerOpenOptions { Title = "选择文件夹", AllowMultiple = false };

        // var mainWindow = (TopLevel.GetTopLevel(this) as Window);
        // bool? result = await StorageProvider.OpenFolderPickerAsync(dialog);

        // TODO: Check dialog result
        if (true) {
            var folderPath = "placeholder";
            var folderName = Path.GetFileName(folderPath);
            var jsonPath = Path.Combine(folderPath, $"{folderName}.json");

            if (!File.Exists(jsonPath))
            {
                var jsonFiles = Directory.GetFiles(folderPath, "*.json");
                if (jsonFiles.Length == 0)
                {
        Console.WriteLine($"[MessageBox] 文件夹内没有找到版本 JSON 文件");
                    return;
                }
                jsonPath = jsonFiles[0];
            }

            ImportVersionFromJson(jsonPath);
        }
    }

    private void ImportVersionFromJson(string jsonPath)
    {
        var versionDir = Path.GetDirectoryName(jsonPath);
        var versionName = Path.GetFileNameWithoutExtension(jsonPath);

        if (versionName == "launcher_profiles")
        {
        Console.WriteLine($"[MessageBox] 这是 PCL 配置文件，不是版本文件");
            return;
        }

        var isIsolated = Directory.Exists(Path.Combine(versionDir!, "mods"))
                      || Directory.Exists(Path.Combine(versionDir!, "config"));

        var rootPath = Path.GetDirectoryName(versionDir)!;

        var instance = new SkyLauncher.Core.Models.MinecraftInstance
        {
            Name = versionName,
            RootPath = rootPath,
            GameDirectory = isIsolated ? versionDir! : rootPath,
            VersionJsonPath = jsonPath,
            VersionId = versionName,
            Source = LauncherSource.SkyLauncher,
            IsManaged = true
        };

        MainViewModel.Instance.AddInstance(instance);
        SelectedInstance = instance;
        OnPropertyChanged(nameof(InstanceList));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}