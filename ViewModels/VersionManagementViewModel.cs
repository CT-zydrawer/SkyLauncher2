using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SkyLauncher.Core.Models;
using SkyLauncher.Views;

namespace SkyLauncher.ViewModels;

public class VersionManagementViewModel : INotifyPropertyChanged
{
    private static VersionManagementViewModel? _instance;
    public static VersionManagementViewModel Instance => _instance ??= new VersionManagementViewModel();

    private SkyLauncher.Core.Models.MinecraftInstance? _selectedInstance;

    public VersionManagementViewModel()
    {
        RefreshCommand = new RelayCommand(ExecuteRefresh);
        ImportVersionCommand = new RelayCommand(async () => await ExecuteImportVersionAsync());
        ImportFolderCommand = new RelayCommand(async () => await ExecuteImportFolderAsync());
        DeleteInstanceCommand = new RelayCommand<SkyLauncher.Core.Models.MinecraftInstance>(ExecuteDeleteInstance);
    }

    public ObservableCollection<SkyLauncher.Core.Models.MinecraftInstance> InstanceList
        => MainViewModel.Instance.InstanceList;

    public SkyLauncher.Core.Models.MinecraftInstance? SelectedInstance
    {
        get => _selectedInstance;
        set
        {
            _selectedInstance = value;
            if (value != null)
                MainViewModel.Instance.SelectedInstance = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedInstanceInfo));
        }
    }

    public string SelectedInstanceInfo
    {
        get
        {
            if (SelectedInstance == null)
                return "未选择实例";

            var inst = SelectedInstance;
            return $"实例：{inst.Name}\n版本ID：{inst.VersionId}\n文件路径：{inst.GameDirectory}\n{(inst.LoaderName ?? "原版")}";
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand ImportVersionCommand { get; }
    public ICommand ImportFolderCommand { get; }
    public ICommand DeleteInstanceCommand { get; }

    #region 命令执行方法

    private void ExecuteRefresh()
    {
        MainViewModel.Instance.ScanInstances();
        OnPropertyChanged(nameof(InstanceList));
    }

    private async Task ExecuteImportVersionAsync()
    {
        try
        {
            var mainWindow = ViewModelHelper.GetMainWindow();
            if (mainWindow == null)
            {
                await ViewModelHelper.ShowMessageAsync("无法获取主窗口", "错误");
                return;
            }

            // 配置文件选择器
            var options = new FilePickerOpenOptions
            {
                Title = "选择版本 JSON 文件",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("JSON 文件")
                    {
                        Patterns = new[] { "*.json" },
                        MimeTypes = new[] { "application/json" }
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

            // 检查用户是否取消了选择
            if (result == null || result.Count == 0)
                return;

            var filePath = result[0].Path.LocalPath;

            // 导入版本
            await ImportVersionFromJsonAsync(filePath);
        }
        catch (Exception ex)
        {
            await ViewModelHelper.ShowMessageAsync($"导入版本失败：{ex.Message}", "错误");
        }
    }

    private async Task ExecuteImportFolderAsync()
    {
        try
        {
            var mainWindow = ViewModelHelper.GetMainWindow();
            if (mainWindow == null)
            {
                await ViewModelHelper.ShowMessageAsync("无法获取主窗口", "错误");
                return;
            }

            // 配置文件夹选择器
            var options = new FolderPickerOpenOptions
            {
                Title = "选择版本文件夹",
                AllowMultiple = false
            };

            // 打开文件夹选择对话框
            var result = await mainWindow.StorageProvider.OpenFolderPickerAsync(options);

            // 检查用户是否取消了选择
            if (result == null || result.Count == 0)
                return;

            var folderPath = result[0].Path.LocalPath;
            var folderName = Path.GetFileName(folderPath);

            // 查找 JSON 文件
            var jsonPath = Path.Combine(folderPath, $"{folderName}.json");

            if (!File.Exists(jsonPath))
            {
                var jsonFiles = Directory.GetFiles(folderPath, "*.json");
                if (jsonFiles.Length == 0)
                {
                    await ViewModelHelper.ShowMessageAsync("文件夹内没有找到版本 JSON 文件", "提示");
                    return;
                }
                jsonPath = jsonFiles[0];
            }

            // 导入版本
            await ImportVersionFromJsonAsync(jsonPath);
        }
        catch (Exception ex)
        {
            await ViewModelHelper.ShowMessageAsync($"导入文件夹失败：{ex.Message}", "错误");
        }
    }

    private async void ExecuteDeleteInstance(SkyLauncher.Core.Models.MinecraftInstance? instance)
    {
        if (instance == null)
            return;

        // 显示确认对话框
        var confirmResult = await ViewModelHelper.ShowConfirmAsync(
            $"确定要删除实例 \"{instance.Name}\" 吗？\n此操作不会删除游戏文件。",
            "确认删除");

        if (confirmResult != ButtonResult.Yes)
            return;

        try
        {
            MainViewModel.Instance.RemoveInstance(instance);

            // 如果删除的是当前选中的，清空选择
            if (SelectedInstance == instance)
                SelectedInstance = null;

            OnPropertyChanged(nameof(InstanceList));

            await ViewModelHelper.ShowMessageAsync($"已删除实例：{instance.Name}", "成功");
        }
        catch (Exception ex)
        {
            await ViewModelHelper.ShowMessageAsync($"删除实例失败：{ex.Message}", "错误");
        }
    }

    private async Task ImportVersionFromJsonAsync(string jsonPath)
    {
        try
        {
            var versionDir = Path.GetDirectoryName(jsonPath);
            if (string.IsNullOrEmpty(versionDir))
            {
                await ViewModelHelper.ShowMessageAsync("无法获取版本目录", "错误");
                return;
            }

            var versionName = Path.GetFileNameWithoutExtension(jsonPath);

            // 检查是否是 PCL 配置文件
            if (versionName == "launcher_profiles")
            {
                await ViewModelHelper.ShowMessageAsync("这是 PCL 配置文件，不是版本文件", "提示");
                return;
            }

            // 检查是否是独立版本（包含 mods 或 config 文件夹）
            var isIsolated = Directory.Exists(Path.Combine(versionDir, "mods"))
                          || Directory.Exists(Path.Combine(versionDir, "config"));

            var rootPath = Path.GetDirectoryName(versionDir) ?? versionDir;

            // 创建实例
            var instance = new SkyLauncher.Core.Models.MinecraftInstance
            {
                Name = versionName,
                RootPath = rootPath,
                GameDirectory = isIsolated ? versionDir : rootPath,
                VersionJsonPath = jsonPath,
                VersionId = versionName,
                Source = LauncherSource.SkyLauncher,
                IsManaged = true
            };

            // 添加到实例列表
            MainViewModel.Instance.AddInstance(instance);

            // 更新 UI
            SelectedInstance = instance;
            OnPropertyChanged(nameof(InstanceList));

            await ViewModelHelper.ShowMessageAsync($"成功导入版本：{versionName}", "成功");
        }
        catch (Exception ex)
        {
            await ViewModelHelper.ShowMessageAsync($"导入版本失败：{ex.Message}", "错误");
        }
    }




    #endregion

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

