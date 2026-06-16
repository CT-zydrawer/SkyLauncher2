using Avalonia.Interactivity;
﻿using Nrk.FluentCore.GameManagement.Mods;
using SkyLauncher.ViewModels;
using System;
using Avalonia;
using Avalonia.Controls;

namespace SkyLauncher.Views
{
    public partial class MinecraftSettingPage : UserControl
    {
        private readonly MinecraftSettingViewModel _viewModel;

        public MinecraftSettingPage()
        {
            InitializeComponent();
        }

        public MinecraftSettingPage(SkyLauncher.Core.Models.MinecraftInstance instance) : this()
        {
            // 创建 ViewModel 并设置 DataContext
            _viewModel = new MinecraftSettingViewModel(instance);
            DataContext = _viewModel;
        }

        private void OpenModFilesFolder(object sender, RoutedEventArgs e)
        {
            _viewModel.OpenModsFolder();
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
}