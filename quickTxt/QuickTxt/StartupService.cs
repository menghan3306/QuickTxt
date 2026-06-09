using Microsoft.Win32;

namespace QuickTxt;

public static class StartupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "QuickTxt";

    public static void Apply(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
            ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);

        if (key is null)
        {
            throw new InvalidOperationException("无法打开 Windows 启动项注册表。");
        }

        if (enabled)
        {
            key.SetValue(ValueName, Quote(Application.ExecutablePath), RegistryValueKind.String);
        }
        else
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
        }
    }

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        var value = key?.GetValue(ValueName)?.ToString();
        return !string.IsNullOrWhiteSpace(value);
    }

    private static string Quote(string path)
    {
        return $"\"{path}\"";
    }
}
