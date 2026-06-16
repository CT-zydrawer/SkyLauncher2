using System;
using Avalonia.Media.Imaging;
using System.IO;
namespace SkyLauncher.Models;

public class ScreenshotItem
{
    public string FilePath { get; set; } = "";

    public string FileName => Path.GetFileName(FilePath);

    public DateTime CreateTime { get; set; }

    public Bitmap? Thumbnail { get; set; }
}