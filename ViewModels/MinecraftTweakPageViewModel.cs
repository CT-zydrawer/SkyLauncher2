using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using Avalonia.Input;

namespace SkyLauncher.ViewModels;

public class ResourcePackItem : INotifyPropertyChanged
{
    private bool _isEnabled;

    public string FileName { get; set; } = string.Empty;

    public string DisplayName =>
        Path.GetFileNameWithoutExtension(FileName);

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value)
                return;

            _isEnabled = value;
            OnPropertyChanged();

        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(
        [CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(name));
    }
}

public class ResourcePackManagerViewModel : INotifyPropertyChanged
{
    private readonly string _optionsFilePath;
    private readonly string _resourcePacksDir;

    public ObservableCollection<ResourcePackItem> DataList { get; } = new();
    public ICommand SaveCommand { get; }

    public ResourcePackManagerViewModel()
    {
        var instance = MainViewModel.Instance.SelectedInstance;
        if (instance == null)
            throw new InvalidOperationException("没有选中的实例");

        _resourcePacksDir = Path.Combine(instance.GameDirectory, "resourcepacks");
        _optionsFilePath = Path.Combine(instance.GameDirectory, "options.txt");

        LoadResourcePacks();
        SaveCommand = new RelayCommand(ExecuteSave);
    }

    private void LoadResourcePacks()
    {
        DataList.Clear();

        if (!Directory.Exists(_resourcePacksDir))
            Directory.CreateDirectory(_resourcePacksDir);

        // 读取已启用的资源包
        var enabled = ReadEnabledResourcePacks();

        // 遍历所有 zip
        foreach (var file in Directory.GetFiles(_resourcePacksDir, "*.zip"))
        {
            DataList.Add(new ResourcePackItem
            {
                FileName = Path.GetFileName(file),
                IsEnabled = enabled.Contains(Path.GetFileName(file))
            });
        }
    }

    private HashSet<string> ReadEnabledResourcePacks()
    {
        var result = new HashSet<string>();
        if (!File.Exists(_optionsFilePath))
            return result;

        try
        {
            foreach (var line in File.ReadAllLines(_optionsFilePath))
            {
                if (!line.StartsWith("resourcePacks:")) continue;

                var json = line.Substring("resourcePacks:".Length).Trim();
                if (string.IsNullOrEmpty(json) || json == "[]") continue;
                var original = JsonSerializer.Deserialize<List<string>>(json);
                var packs = JsonSerializer.Deserialize<List<string>>(json);
                
                if (packs != null)
                {
                    foreach (var p in packs)
                        result.Add(p.StartsWith("file/") ? p.Substring("file/".Length) : p);
                }
                
                break;
            }
        }
        catch { }

        return result;
    }

    public async void OpenResourcePackFolder()
    {
        if (Directory.Exists(_resourcePacksDir))
            System.Diagnostics.Process.Start("explorer.exe", _resourcePacksDir);
        else
            await ViewModelHelper.ShowMessageAsync("文件夹不存在或文件系统未适配", "错误");
    }

    private async void ExecuteSave()
    {

        var enabledList = DataList
       .Where(p => p.IsEnabled)
       .Select(p => $"file/{p.FileName}")
       .ToList();

        var jsonArray = JsonSerializer.Serialize(enabledList);

        // 更新 options.txt
        var lines = File.Exists(_optionsFilePath)
            ? File.ReadAllLines(_optionsFilePath).ToList()
            : new List<string>();

        bool found = false;
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].StartsWith("resourcePacks:"))
            {
                lines[i] = $"resourcePacks:{jsonArray}";
                found = true;
                break;
            }
        }
        if (!found)
            lines.Add($"resourcePacks:{jsonArray}");

        File.WriteAllLines(_optionsFilePath, lines);

        await ViewModelHelper.ShowMessageAsync("更改已经保存", "信息");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}