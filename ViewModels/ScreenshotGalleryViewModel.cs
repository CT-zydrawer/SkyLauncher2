using SkyLauncher.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Avalonia;
using Avalonia.Media.Imaging;
using System;

namespace SkyLauncher.ViewModels;

public class ScreenshotGalleryViewModel : INotifyPropertyChanged
{
    private readonly string _screenshotFolder;

    public ScreenshotGalleryViewModel()
    {
        _screenshotFolder = MainViewModel.Instance.GetScreenshotFolder();
        Screenshots = new ObservableCollection<ScreenshotItem>();
        LoadScreenshots();
    }

    public ObservableCollection<ScreenshotItem> Screenshots { get; }

    public string ScreenshotFolderPath => _screenshotFolder;

    public int ScreenshotCount => Screenshots.Count;

    public bool HasScreenshots => Screenshots.Count > 0;

    public string EmptyMessage => $"截图文件夹为空\n{_screenshotFolder}";

    public async void LoadScreenshots()
    {
        Screenshots.Clear();

        if (!Directory.Exists(_screenshotFolder))
        {
            OnPropertyChanged(nameof(HasScreenshots));
            return;
        }
        
        var imageFiles = Directory.GetFiles(_screenshotFolder)
            .Where(IsImageFile)
            .OrderByDescending(File.GetLastWriteTime)
            .Take(80);

        foreach (var file in imageFiles)
        {
            try
            {
                var bitmap = new Bitmap(file);

                Screenshots.Add(new ScreenshotItem
                {
                    FilePath = file,
                    CreateTime = File.GetLastWriteTime(file),
                    Thumbnail = bitmap
                });
            }
            catch
            {
                await ViewModelHelper.ShowMessageAsync("无法加载", "错误");
            }
        }

        OnPropertyChanged(nameof(ScreenshotCount));
        OnPropertyChanged(nameof(HasScreenshots));
    }

    public void OpenScreenshotFolder()
    {
        if (Directory.Exists(_screenshotFolder))
        {
            System.Diagnostics.Process.Start("explorer.exe", _screenshotFolder);
        }
    }

    private static bool IsImageFile(string path)
    {
        var ext = Path.GetExtension(path).ToLower();

        return ext is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }


}