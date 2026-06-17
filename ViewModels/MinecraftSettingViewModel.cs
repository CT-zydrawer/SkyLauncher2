using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Nrk.FluentCore.GameManagement.Mods;
using SkyLauncher.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SkyLauncher.ViewModels
{

    public partial class MinecraftSettingViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<MinecraftMod> _mods = new ObservableCollection<MinecraftMod>();

        private MinecraftInstance _instance;
        private readonly string modsFolder;
        //private bool _isEnabled;
        private string _minecraftName = "未知";
        public string MinecraftName
        {
            get => _minecraftName;
            set => SetProperty(ref _minecraftName, value);
        }

        public MinecraftSettingViewModel(MinecraftInstance instance)
        {
            if (instance != null)
            {
                _instance = instance;

                modsFolder = _instance.ModsPath;
                MinecraftName = _instance.Name;
                LoadMods();
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("错误!", "没有实例被选择!", ButtonEnum.Ok);
                
            }
        }

        

        [RelayCommand]
        private async void LoadMods()
        {
            Mods.Clear();

            if (!Directory.Exists(modsFolder))
            {
                return;
            }

            // 使用 FluentCore 的 ModManager 枚举模组
            await foreach (var mod in ModManager.EnumerateModsAsync(modsFolder))
            {
                // 如果 DisplayName 为空，使用文件名
                if (string.IsNullOrEmpty(mod.DisplayName))
                {
                    mod.DisplayName = Path.GetFileNameWithoutExtension(mod.AbsolutePath);
                }
                Mods.Add(mod);
            }
        }

        [RelayCommand]
        public async void ToggleModEnabled(MinecraftMod mod)
        {
            try
            {
                // 使用 FluentCore 的扩展方法切换模组状态
                mod.Switch(!mod.IsEnabled);

                // 刷新集合中的项（触发 UI 更新）
                var index = Mods.IndexOf(mod);
                if (index >= 0)
                {
                    Mods[index] = mod;
                }
            }
            catch (Exception ex)
            {
                await ViewModelHelper.ShowMessageAsync($"切换失败,检查文件是否被占用{ex}", "错误");
            }
        }

        [RelayCommand]
        public async void DeleteMod(MinecraftMod mod)
        {
            await ViewModelHelper.ShowMessageAsync("确定要删除吗?这个操作将不可逆!", "警告");

            // TODO: Check dialog result
            if (true) {
                try
                {
                    // 使用 FluentCore 的扩展方法删除模组
                    mod.Delete();
                    Mods.Remove(mod);
                }
                catch (Exception ex)
                {
                    await ViewModelHelper.ShowMessageAsync($"删除失败{ex.Message}", "错误");
                }
            }
        }

        [RelayCommand]
        public async void OpenModsFolder()
        {
            if (Directory.Exists(modsFolder))
            {
                System.Diagnostics.Process.Start("explorer.exe", modsFolder);
            }
            else
            {
                await ViewModelHelper.ShowMessageAsync("无法加载", "错误");
            }
        }
    }
}