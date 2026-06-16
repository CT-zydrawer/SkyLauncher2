using Avalonia.Interactivity;
﻿
using SkyLauncher.Core.Models;
using SkyLauncher.Views;
using System;
using System.IO;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;

using Avalonia.Controls.Shapes;
using System.Linq;

namespace SkyLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // 用一个字段记录当前显示的页面
        private UserControl _currentPage;
        private LauncherSettings _settings;
        public event EventHandler<double> OpacityChanged;
        public Views.ToolboxPage _toolboxInstance;

        public MainWindow()
        {
            InitializeComponent();

            // 加载设置
            _settings = LauncherSettings.Load();

            // 窗口加载时默认显示主页并应用保存的设置
            Loaded += (s, e) =>
            {
                GoToMainPage();
                ApplySavedSettings();
                ApplyInitialOpacity();
            };

            // 订阅事件
            ToolboxPage.ThemeColorChanged += OnThemeColorChanged;
            ToolboxPage.BackgroundImagePathChanged += OnBackgroundImagePathChanged;
        }

        /// <summary>
        /// 这部分代码负责在主窗口加载时应用用户之前保存的设置，包括主题颜色和背景图片。它从LauncherSettings中读取保存的设置，并尝试将它们应用到主窗口的UI元素上。如果保存的设置无效或加载失败，代码会安全地处理异常并继续使用默认设置。
        /// </summary>
        #region
        private void ApplySavedSettings()
        {
            // 应用保存的主题颜色
            if (!string.IsNullOrEmpty(_settings.ThemeColorSetting))
            {
                try
                {
                    // 解析保存的颜色字符串（格式：#AARRGGBB）
                    string colorHex = _settings.ThemeColorSetting.TrimStart('#');
                    byte a = Convert.ToByte(colorHex.Substring(0, 2), 16);
                    byte r = Convert.ToByte(colorHex.Substring(2, 2), 16);
                    byte g = Convert.ToByte(colorHex.Substring(4, 2), 16);
                    byte b = Convert.ToByte(colorHex.Substring(6, 2), 16);

                    Color savedColor = Color.FromArgb(a, r, g, b);
                    mainGrid.Background = new SolidColorBrush(savedColor);
                }
                catch
                {
                    // 如果颜色解析失败，使用默认颜色
                }
            }

            // 应用保存的背景图片
            if (!string.IsNullOrEmpty(_settings.PictureBackgroundPath) &&
                File.Exists(_settings.PictureBackgroundPath))
            {
                try
                {
                    ImageShow.Source = new Bitmap(_settings.PictureBackgroundPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MessageBox] 加载保存的背景图片失败：{ex.Message}");
                }
            }
        }
        //来自ToolboxPage的事件处理方法
        private void OnThemeColorChanged(Color newColor)
        {

            Dispatcher.Invoke(() =>
            {
                mainGrid.Background = new SolidColorBrush(newColor);
            });
        }
        private void ApplyInitialOpacity()
        {
            var settings = LauncherSettings.Load();
            if (settings != null)
            {
                this.Opacity = settings.LauncherOpacity;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // 取消静态订阅
            ToolboxPage.ThemeColorChanged -= OnThemeColorChanged;
            ToolboxPage.BackgroundImagePathChanged -= OnBackgroundImagePathChanged;

            // 如果有订阅到某个 ToolboxPage 实例，也取消订阅
            if (_toolboxInstance != null)
            {
                _toolboxInstance.OpacityChanged -= OnOpacityChanged;
                _toolboxInstance = null;
            }

            base.OnClosed(e);
        }

        private void OnOpacityChanged(object? sender, double opacity)
        {
            // 更新透明度
            ContentArea.Opacity = opacity;
            NavigatePannel.Opacity = opacity;
            this.Opacity = 1;
        }

        private void OnBackgroundImagePathChanged(string newPath)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    ImageShow.Source = new Bitmap(newPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MessageBox] 加载图片失败：{ex.Message}");
                }
            });
        }
        #endregion
        //
        private Grid ContentGrid => mainGrid.Children.OfType<Grid>().FirstOrDefault();

        // 导航方法
        

        /*private void UpdateButtonStyle(Button activeButton)
        {
            var stackPanel = mainGrid.Children.OfType<StackPanel>().FirstOrDefault();
            if (stackPanel == null) return;

            foreach (var child in stackPanel.Children)
            {
                if (child is Button btn)
                {
                    btn.Style = this.TryFindResource("ButtonDefault") as Style;
                }
            }

            if (activeButton != null)
            {
                activeButton.Style = this.TryFindResource("ButtonPrimary") as Style;
            }
        }*/

        // 各个导航方法
        private void GoToMainPage(object sender = null, RoutedEventArgs e = null)
        {
            NavigateToPage(() => new Views.MainPage());
        }

        private void GoToConfigPage(object sender = null, RoutedEventArgs e = null)
        {
            NavigateToPage(() => new Views.ConfigPage());
        }

        private void GoToAboutPage(object sender = null, RoutedEventArgs e = null)
        {
            NavigateToPage(() => new Views.AboutPage());
        }

        private void GoToVersionManagementPage(object sender = null, RoutedEventArgs e = null)
        {
            NavigateToPage(() => new Views.VersionManagementPage(this));
        }

        private void GoToDownloadPage(object sender = null, RoutedEventArgs e = null)
        {
            NavigateToPage(() => new Views.DownloadPage());
        }

        public void GoToToolboxPage(object sender = null, RoutedEventArgs e = null)
        {
            NavigateToPage(() => {
                var toolbox = new Views.ToolboxPage();
                toolbox.OpacityChanged += OnOpacityChanged; 
                return toolbox;
            });
        }

    }
}