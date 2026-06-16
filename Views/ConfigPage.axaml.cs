using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
﻿using Nrk.FluentCore.Environment;
using SkyLauncher.Core.Models;
using SkyLauncher.Models;
using SkyLauncher.ViewModels;
using System;
using System.Diagnostics;
using System.Runtime;
namespace SkyLauncher.Views;

public partial class ConfigPage : UserControl
{
    private readonly ConfigPageViewModel _viewModel;
    private LauncherSettings _settings;
    private MainWindow Window;

    public ConfigPage()
    {
        
        InitializeComponent();
        _viewModel = new ConfigPageViewModel();
        DataContext = _viewModel;
        _settings = LauncherSettings.Load();
        Loaded += ConfigPage_Loaded;
    }

    private void ConfigPage_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel.LoadData();
    }

    /*private void AddNewJava(object sender, RoutedEventArgs e)
    {
        _viewModel.AddJavaCommand.Execute(null);
    }*/
    private void OpenScreenshotGallery(object sender, TappedEventArgs e)
    {
        var mainWindow = TopLevel.GetTopLevel(this) as MainWindow;

        if (mainWindow != null)
        {
            mainWindow.NavigateToPage(() => new Views.ScreenshotGallery());
        }
    }
    private void OpenResourcePackManager(object sender, TappedEventArgs e)
    {
        var mainWindow = TopLevel.GetTopLevel(this) as MainWindow;

        if (mainWindow != null)
        {
            mainWindow.NavigateToPage(() => new Views.ResourcePackManager());
        }
    }
    public async void  GoToMinecraftSettingPage(object sender =null, TappedEventArgs e=null)
    {
        var mainWindow = TopLevel.GetTopLevel(this) as MainWindow;
        var selectedInstance = MainViewModel.Instance.SelectedInstance;

        if (mainWindow != null||selectedInstance !=null)
            {
                mainWindow.NavigateToPage(() => new Views.MinecraftSettingPage(selectedInstance));
            }
        else {
            var box = MessageBoxManager.GetMessageBoxStandard("错误!", "没有实例被选择!", ButtonEnum.Ok);
            await box.ShowAsync();
        }
    }
    private void OpenShaderpackManage(object sender, TappedEventArgs e)
    {
        var mainWindow = TopLevel.GetTopLevel(this) as MainWindow;

        if (mainWindow != null)
        {
            mainWindow.NavigateToPage(() => new Views.ShaderPackManage());
        }
    }

    private void OpenSchematicsManage(object sender, TappedEventArgs e)
    {
        var mainWindow = TopLevel.GetTopLevel(this) as MainWindow;

        if (mainWindow != null)
        {
            mainWindow.NavigateToPage(() => new Views.SchematicsManage());
        }
    }

}