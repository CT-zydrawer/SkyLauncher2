using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
﻿using SkyLauncher.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyLauncher.Views
{
    /// <summary>
    /// ShaderPackManage.xaml 的交互逻辑
    /// </summary>
    public partial class ShaderPackManage : UserControl
    {
        //private readonly string _shaderPacksDir;
        public ObservableCollection<ResourcePackItem> DataList { get; } = new();
        public ShaderPackManage()
        {
            InitializeComponent();
            LoadShaderPacks();
            this.DataContext = this;
        }
        private string ShaderPacksDir
        {
            get
            {
                var instance = MainViewModel.Instance.SelectedInstance;
                return instance != null
                    ? System.IO.Path.Combine(instance.GameDirectory, "shaderpacks")
                    : string.Empty;
            }
        }
        public async void OpenShaderPackFolder(object sender, RoutedEventArgs e)
        {
            var instance = MainViewModel.Instance.SelectedInstance;
            if (instance == null)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("设置", "实例未选择", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            var shaderPacksDir = System.IO.Path.Combine(instance.GameDirectory, "shaderpacks");
            if (!System.IO.Directory.Exists(shaderPacksDir))
            {
                System.IO.Directory.CreateDirectory(shaderPacksDir);
            }
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = shaderPacksDir,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("错误", $"发生错误{ex.Message}", ButtonEnum.Ok);
                await box.ShowAsync();
                
            }
        }
        public void LoadShaderPacks()
        {
            DataList.Clear();

            if (!Directory.Exists(ShaderPacksDir))
                Directory.CreateDirectory(ShaderPacksDir);

            // 读取已启用的资源包

            // 遍历所有 zip
            foreach (var file in Directory.GetFiles(ShaderPacksDir, "*.zip"))
            {
                DataList.Add(new ResourcePackItem
                {
                    FileName = System.IO.Path.GetFileName(file),
                   
                });
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
    }
}
