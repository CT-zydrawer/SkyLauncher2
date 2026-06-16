using Avalonia.Controls;
using SkyLauncher.ViewModels;

namespace SkyLauncher.Views;

public partial class DownloadPage : UserControl
{
    public DownloadPage()
    {
        InitializeComponent();
        DataContext = new DownloadViewModel();
    }
}