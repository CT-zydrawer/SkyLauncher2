using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SkyLauncher.Core.Services;

namespace SkyLauncher.ViewModels;

public class DownloadViewModel : INotifyPropertyChanged
{
    private string _statusText = "等待选择整合包...";
    private double _progress;
    private bool _isInstalling;
    private CancellationTokenSource? _cts;

    private readonly ModpackInstallService _installService;

    public DownloadViewModel()
    {
        PickPackageCommand = new RelayCommand(ExecutePickPackage);
        CancelCommand = new RelayCommand(ExecuteCancel);

        _installService = new ModpackInstallService();
        _installService.StatusChanged += (s) =>
        {
            Dispatcher.UIThread.Post(() => StatusText = s);
        };
        _installService.ProgressChanged += (p) =>
        {
            Dispatcher.UIThread.Post(() => Progress = p);
        };
    }

    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    public double Progress
    {
        get => _progress;
        set { _progress = value; OnPropertyChanged(); }
    }

    public bool IsInstalling
    {
        get => _isInstalling;
        set { _isInstalling = value; OnPropertyChanged(); }
    }

    public ICommand PickPackageCommand { get; }
    public ICommand CancelCommand { get; }

    private async void ExecutePickPackage()
    {
        var mainWindow = ViewModelHelper.GetMainWindow();
        if (mainWindow == null)
        {
            await ViewModelHelper.ShowMessageAsync("无法获取主窗口", "错误");
            return;
        }

        var options = new FilePickerOpenOptions
        {
            Title = "选择版本 JSON 文件",
            AllowMultiple = false,
            FileTypeFilter = new[]
                {
                    new FilePickerFileType("Curseforge整合包文件")
                    {
                        Patterns = new[] { "*.zip" },
                        
                    },
                    new FilePickerFileType("Modrinth整合包文件")
                    {
                        Patterns = new[] { "*.mrpack" },
                    }
                }
        };

        // 打开文件夹选择对话框
        var result = await mainWindow.StorageProvider.OpenFilePickerAsync(options);

        // 检查用户是否取消了选择
        if (result == null || result.Count == 0)
            return;

        IsInstalling = true;
        Progress = 0;
        _cts = new CancellationTokenSource();

        try
        {
            var filePath = options.SuggestedFileName;
            var minecraftFolder = MainViewModel.Instance.GetActualMinecraftFolder();
            var javaPath = MainViewModel.Instance.JavaExecutablePath;

            if (string.IsNullOrEmpty(javaPath))
            {
                await ViewModelHelper.ShowMessageAsync("请先选择Java", "错误");
                return;
            }

            string ext = Path.GetExtension(filePath).ToLower();

            if (ext == ".mrpack")
            {
                await _installService.InstallModrinthAsync(filePath, minecraftFolder, javaPath, _cts.Token);
            }
            else if (ext == ".zip")
            {
                await _installService.InstallCurseForgeAsync(filePath, minecraftFolder, javaPath, _cts.Token);
            }

            // 刷新实例列表
            MainViewModel.Instance.ScanInstances(minecraftFolder);

            StatusText = "安装完成！请到版本管理页面查看新实例。";
            Console.WriteLine("[MessageBox] 整合包安装完成！");
        }
        catch (OperationCanceledException)
        {
            StatusText = "已取消安装";
        }
        catch (Exception ex)
        {
            StatusText = "安装失败";
            Console.WriteLine($"[MessageBox] 安装失败: {ex.Message}");
        }
        finally
        {
            IsInstalling = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void ExecuteCancel()
    {
        _cts?.Cancel();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}