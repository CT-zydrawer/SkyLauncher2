using Avalonia.Interactivity;
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

        private void ConfirmSelectPic(object sender, RoutedEventArgs e)
        {
            string imagePath = null /* TODO: ImageSelector */;

            if (imagePath == null)
            {
                Console.WriteLine("[MessageBox] 请选择一张图片！");
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

        private void ConfirmOpacity(object sender, RoutedEventArgs e)
        {
            double opacity = OpacitySlider.Value;

            if (_settings != null)
            {
                _settings.LauncherOpacity = (float)opacity;
                _settings.Save();
                OpacityChanged?.Invoke(this, opacity);
                Debug.WriteLine(_settings.LauncherOpacity);
                Debug.WriteLine(_settings.LauncherOpacity.GetType());
                Console.WriteLine($"[MessageBox] 已设置启动器不透明度为: {opacity:F2}");
               
            }
        }

    }
}