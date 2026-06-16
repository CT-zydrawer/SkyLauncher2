using SkyLauncher.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkyLauncher.Core.Services;

public static class JavaRuntimeService
{
    /// <summary>
    /// 扫描系统中已安装的 Java
    /// </summary>
    public static List<JavaRuntime> ScanInstalledJava()
    {
        var result = new List<JavaRuntime>();

        // 检查常见安装目录
        var commonPaths = new List<string>
        {
            @"C:\Program Files\Java",
            @"C:\Program Files (x86)\Java",
            @"C:\Program Files\Eclipse Adoptium",
            @"C:\Program Files\Microsoft",
            @"C:\Program Files\Amazon Corretto",
            @"C:\Program Files\Zulu",
        };

        foreach (var basePath in commonPaths)
        {
            if (Directory.Exists(basePath))
            {
                foreach (var dir in Directory.GetDirectories(basePath))
                {
                    var javawPath = Path.Combine(dir, "bin", "javaw.exe");
                    if (File.Exists(javawPath))
                    {
                        result.Add(new JavaRuntime
                        {
                            Path = dir,
                            ExecutablePath = javawPath,
                            Version = GetJavaVersion(javawPath)
                        });
                    }
                }
            }
        }

        // 检查 JAVA_HOME
        var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome) && Directory.Exists(javaHome))
        {
            var javawPath = Path.Combine(javaHome, "bin", "javaw.exe");
            if (File.Exists(javawPath) && !result.Any(r => r.ExecutablePath == javawPath))
            {
                result.Add(new JavaRuntime
                {
                    Path = javaHome,
                    ExecutablePath = javawPath,
                    Version = GetJavaVersion(javawPath)
                });
            }
        }

        return result.OrderByDescending(r => r.Version).ToList();
    }

    /// <summary>
    /// 手动导入 Java（从用户选择的文件路径）
    /// </summary>
    public static JavaRuntime? ImportJava(string javaExecutablePath)
    {
        if (!File.Exists(javaExecutablePath))
            return null;

        var jdkRoot = Path.GetDirectoryName(Path.GetDirectoryName(javaExecutablePath));
        return new JavaRuntime
        {
            Path = jdkRoot ?? string.Empty,
            ExecutablePath = javaExecutablePath,
            Version = GetJavaVersion(javaExecutablePath)
        };
    }

    /// <summary>
    /// 获取 Java 版本号
    /// </summary>
    public static string GetJavaVersion(string javaExecutablePath)
    {
        if (!File.Exists(javaExecutablePath))
            return "未知";

        // 方法1：解析 release 文件（更快）
        var jdkRoot = Path.GetDirectoryName(Path.GetDirectoryName(javaExecutablePath));
        if (jdkRoot != null)
        {
            var releaseFile = Path.Combine(jdkRoot, "release");
            if (File.Exists(releaseFile))
            {
                var content = File.ReadAllText(releaseFile);
                var match = Regex.Match(content, @"JAVA_VERSION=""([^""]+)""");
                if (match.Success)
                    return match.Groups[1].Value;
            }
        }

        // 方法2：执行 java -version
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = javaExecutablePath,
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardError.ReadToEnd() + process.StandardOutput.ReadToEnd();
            process.WaitForExit(3000);

            var versionMatch = Regex.Match(output, @"version ""([^""]+)""");
            if (versionMatch.Success)
                return versionMatch.Groups[1].Value;
        }
        catch {
        Console.WriteLine($"[MessageBox] 获取 Java 版本失败");
        }

        return "未知版本";
    }
}