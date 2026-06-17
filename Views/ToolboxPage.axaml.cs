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
﻿

using SkyLauncher.Core.Models;
using SkyLauncher.Service;
using SkyLauncher.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SkyLauncher.Views
{
    /// <summary>
    /// ToolboxPage.xaml 的交互逻辑
    /// </summary>
    public partial class ToolboxPage : UserControl, INotifyPropertyChanged
    {
        private LauncherSettings _settings;
        private readonly string _token;
        public event EventHandler<double> OpacityChanged;
        public ToolboxPage()
        {
            InitializeComponent();
            _settings = LauncherSettings.Load(); // 先加载设置
            //_token = token;
            if (_settings != null)
            {
                OpacitySlider.Value = _settings.LauncherOpacity;
                Debug.WriteLine(_settings.LauncherOpacity);
            }
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //private bool isFirstLoad = true;

        /*private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isFirstLoad)
            {
                isFirstLoad = false;
                return;
            }
            if (MainTabControl.SelectedIndex == 0)
            {
                var currentContent = ThemeTransition.Content;
                ThemeTransition.Content = null;
                ThemeTransition.Content = currentContent;
            }
        }*/
        public string ThemeColor
        {
            get => _settings?.ThemeColorSetting;
            set
            {
                if (_settings != null)
                {
                    _settings.ThemeColorSetting = value;
                    _settings.Save();
                    OnPropertyChanged();
                }
            }
        }

        private void SwitchToLight(object sender, RoutedEventArgs e)
        {
            /*var dicts = Application.Current.Resources.MergedDictionaries;
            for (int i = dicts.Count - 1; i >= 0; i--)
            {
                var source = dicts[i].Source?.ToString();
                if (source != null && source.Contains("Skin"))
                {
                    dicts.RemoveAt(i);
                }
            }
            Application.Current.Resources.MergedDictionaries.Add(
                ResourceHelper.GetSkin(SkinType.Default));
            var brush = Application.Current.FindResource("RegionBrush") as SolidColorBrush;
            Debug.WriteLine(brush.Color);
            dicts.Add(ResourceHelper.GetSkin(SkinType.Default));

            */
        }

        private void SwitchToDark(object sender, RoutedEventArgs e)
        {
            /*var dicts = Application.Current.Resources.MergedDictionaries;
            for (int i = dicts.Count - 1; i >= 0; i--)
            {
                var source = dicts[i].Source?.ToString();
                if (source != null && source.Contains("Skin"))
                {
                    dicts.RemoveAt(i);
                }
            }
            Application.Current.Resources.MergedDictionaries.Add(
                ResourceHelper.GetSkin(SkinType.Dark));
            var brush = Application.Current.FindResource("RegionBrush") as SolidColorBrush;
            Debug.WriteLine(brush.Color);
            dicts.Add(ResourceHelper.GetSkin(SkinType.Dark));

            */
        }

        public Avalonia.Media.IImage BackgroundImagePath { get; set; }

        public static event Action<Color> ThemeColorChanged;
        public static event Action<String> BackgroundImagePathChanged;

        private void ColorPicker_OnSelectedColorChanged(object sender, ColorChangedEventArgs e)
        {
            if (e.NewColor != null)
            {
                Color selectedColor = e.NewColor;
                ThemeColor = $"#{selectedColor.A:X2}{selectedColor.R:X2}{selectedColor.G:X2}{selectedColor.B:X2}";
                //System.Diagnostics.Debug.WriteLine($"OnSelectedColorChanged - A:{selectedColor.A} R:{selectedColor.R} G:{selectedColor.G} B:{selectedColor.B}");
                ThemeColorChanged?.Invoke(selectedColor);
            }
        }

        private async void ConfirmSelectPic(object sender, RoutedEventArgs e)
        {
            string imagePath = null /* TODO: ImageSelector */;

            if (imagePath == null)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("提示", "路径错误,无法找到文件", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            else
            {
                BackgroundImagePathChanged?.Invoke(imagePath);
                IImage imageSource = new Bitmap(imagePath);
                BackgroundImagePath = imageSource;

                // 同时保存图片路径到设置
                if (_settings != null)
                {
                    _settings.PictureBackgroundPath = imagePath;
                    _settings.Save();
                }
            }
        }

        private void ColorPicker_ColorConfirmed(object sender, ColorChangedEventArgs e)
        {
            if (e.NewColor != null)
            {
                //Color c = e.NewColor;
                //System.Diagnostics.Debug.WriteLine($"ColorConfirmed - A:{c.A} R:{c.R} G:{c.G} B:{c.B}");
                Color selectedColor = e.NewColor;
                ThemeColor = $"#{selectedColor.A:X2}{selectedColor.R:X2}{selectedColor.G:X2}{selectedColor.B:X2}";
                Console.WriteLine($"[MessageBox] 已选择颜色: {ThemeColor}");
                //public RelayCommand InfoCmd => new(() => Growl.Info(Properties.Langs.Lang.GrowlInfo, _token));
            }
        }

        private async void ConfirmOpacity(object sender, RoutedEventArgs e)
        {
            double opacity = OpacitySlider.Value;

            if (_settings != null)
            {
                _settings.LauncherOpacity = (float)opacity;
                _settings.Save();
                OpacityChanged?.Invoke(this, opacity);
                Debug.WriteLine(_settings.LauncherOpacity);
                Debug.WriteLine(_settings.LauncherOpacity.GetType());
                var box = MessageBoxManager.GetMessageBoxStandard("设置", $"透明度已经选择{opacity:F2}", ButtonEnum.Ok);
                await box.ShowAsync();
               
            }
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
  
            var tabControl = sender as TabControl;
            var selectedTabItem = tabControl?.SelectedItem as TabItem;

            if (selectedTabItem != null)
            {
                
                var transitionControl = FindTransitioningContentControl(selectedTabItem);

                if (transitionControl != null)
                {
                    var content = transitionControl.Content;
                    transitionControl.Content = null;
                    transitionControl.Content = content;
                }
            }
        }

        private TransitioningContentControl? FindTransitioningContentControl(TabItem tabItem)
        {
            if (tabItem.Content is TransitioningContentControl transition)
            {
                return transition;
            }
            return null;
        }

    }
}