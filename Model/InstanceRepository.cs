using SkyLauncher.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SkyLauncher.Core.Services;

public static class InstanceRepository
{
    private static readonly string Folder =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "SkyLauncher");

    private static readonly string FilePath =
        Path.Combine(Folder, "instances.json");

    public static List<MinecraftInstance> Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);

                return JsonSerializer.Deserialize<List<MinecraftInstance>>(json)
                    ?? new();
            }
        }
        catch
        {
        }

        return new();
    }

    public static void Save(List<MinecraftInstance> instances)
    {
        try
        {
            Directory.CreateDirectory(Folder);

            var json = JsonSerializer.Serialize(
                instances,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(FilePath, json);
        }
        catch
        {
        }
    }
}