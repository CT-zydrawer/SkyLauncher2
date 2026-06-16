using Avalonia.Interactivity;
﻿using Avalonia;
using Avalonia.Controls;
using SkyLauncher.ViewModels;

namespace SkyLauncher.Views;

public partial class ScreenshotGallery : UserControl
{
    private readonly ScreenshotGalleryViewModel _viewModel;

    public ScreenshotGallery()
    {
        InitializeComponent();
        _viewModel = new ScreenshotGalleryViewModel();
        DataContext = _viewModel;
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.LoadScreenshots();
    }

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.OpenScreenshotFolder();
    }

    private void GoBack(object sender, RoutedEventArgs e)
    {
        var mainWindow = TopLevel.GetTopLevel(this) as MainWindow;

        if (mainWindow != null)
        {
            mainWindow.NavigateToPage(() => new Views.ConfigPage());
        }
    }
}