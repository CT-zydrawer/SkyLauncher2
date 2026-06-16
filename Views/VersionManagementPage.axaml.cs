using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SkyLauncher.ViewModels;
using System;

namespace SkyLauncher.Views;

public partial class VersionManagementPage : UserControl
{
    private MainWindow Window;
    public VersionManagementPage(MainWindow window)
    {
        Window = window;
        InitializeComponent();
        DataContext = VersionManagementViewModel.Instance;
        Loaded += VersionManagementPage_Loaded;
    }

    private void VersionManagementPage_Loaded(object sender, RoutedEventArgs e)
    {
        // 页面加载时自动扫描（如果还没扫过）
        if (VersionManagementViewModel.Instance.InstanceList.Count == 0)
        {
            VersionManagementViewModel.Instance.RefreshCommand.Execute(null);
        }
    }
    private async void DeleteInstance_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is SkyLauncher.Core.Models.MinecraftInstance instance)
        {
            if (instance != null)
            {
                VersionManagementViewModel.Instance.DeleteInstanceCommand.Execute(instance);
            }
            else {
                var box = MessageBoxManager.GetMessageBoxStandard("错误", "未找到实例信息", ButtonEnum.Ok);
                await box.ShowAsync();
            }
        }

    }

    /*private void GoToMinecraftSettingPage(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is SkyLauncher.Core.Models.MinecraftInstance instance){
            Window.ContentArea.Content = new Views.MinecraftSettingPage(instance);
        }
    }*/
}