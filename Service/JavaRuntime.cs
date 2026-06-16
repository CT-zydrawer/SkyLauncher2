namespace SkyLauncher.Core.Models;

/// <summary>
/// Java 运行时信息
/// </summary>
public class JavaRuntime
{
    public string Path { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 用于 ComboBox 等控件显示
    /// </summary>
    public string DisplayText => $"Java {Version} — {ExecutablePath}";
}