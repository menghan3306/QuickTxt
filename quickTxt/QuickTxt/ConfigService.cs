using System.Text.Json;

namespace QuickTxt;

public sealed class ConfigService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string AppDirectory { get; }

    public string ConfigPath { get; }

    public ConfigService()
    {
        AppDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "QuickTxt");
        ConfigPath = Path.Combine(AppDirectory, "config.json");
    }

    public AppConfig Load()
    {
        Directory.CreateDirectory(AppDirectory);

        if (!File.Exists(ConfigPath))
        {
            var defaultConfig = Normalize(new AppConfig());
            Save(defaultConfig);
            return defaultConfig;
        }

        try
        {
            var json = File.ReadAllText(ConfigPath);
            var config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
            config = Normalize(config);
            Save(config);
            return config;
        }
        catch
        {
            BackupBrokenConfig();

            var defaultConfig = Normalize(new AppConfig());
            Save(defaultConfig);
            return defaultConfig;
        }
    }

    public void Save(AppConfig config)
    {
        Directory.CreateDirectory(AppDirectory);
        config = Normalize(config);

        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(ConfigPath, json);
    }

    public static string GetDefaultSaveDirectory()
    {
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (string.IsNullOrWhiteSpace(documents))
        {
            documents = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        return Path.Combine(documents, "QuickTxt");
    }

    private static AppConfig Normalize(AppConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.SaveDirectory))
        {
            config.SaveDirectory = GetDefaultSaveDirectory();
        }

        if (config.Hotkey is null)
        {
            config.Hotkey = new HotkeyConfig();
        }

        if (config.Hotkey.Modifiers is null || config.Hotkey.Modifiers.Count == 0)
        {
            config.Hotkey.Modifiers = ["Ctrl", "Alt"];
        }

        if (string.IsNullOrWhiteSpace(config.Hotkey.Key))
        {
            config.Hotkey.Key = "N";
        }

        if (string.IsNullOrWhiteSpace(config.FileNamePattern))
        {
            config.FileNamePattern = "yyyy-MM-dd_HH-mm-ss";
        }

        config.MaxRecentFiles = Math.Clamp(config.MaxRecentFiles, 1, 50);
        return config;
    }

    private void BackupBrokenConfig()
    {
        if (!File.Exists(ConfigPath))
        {
            return;
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupPath = Path.Combine(AppDirectory, $"config.broken_{timestamp}.json");
        File.Move(ConfigPath, backupPath, overwrite: true);
    }
}
