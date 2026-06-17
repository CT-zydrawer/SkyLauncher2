using Avalonia.Interactivity;
﻿using SkyLauncher.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;

using Avalonia.Controls.Shapes;

namespace SkyLauncher.Views
{
    /// <summary>
    /// ResourcePackManager.xaml 的交互逻辑
    /// </summary>
    public partial class ResourcePackManager : UserControl
    {
        private readonly ResourcePackManagerViewModel _viewModel;
        public ResourcePackManager()
        {
            
        InitializeComponent();
            try
            {
                _viewModel = new ResourcePackManagerViewModel();
                DataContext = _viewModel;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"FuckError {ex.Message}");
            }
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            var mainWindow = TopLevel.GetTopLevel(this) as MainWindow;

            if (mainWindow != null)
            {
                mainWindow.NavigateToPage(() => new Views.ConfigPage());
            }
        }

        private void OpenResourcepackFolder(object sender, RoutedEventArgs e)
        {
            _viewModel.OpenResourcePackFolder();
        }
    }
}
