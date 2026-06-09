namespace QuickTxt;

public sealed class AppConfig
{
    public string SaveDirectory { get; set; } = ConfigService.GetDefaultSaveDirectory();

    public HotkeyConfig Hotkey { get; set; } = new();

    public string FileNamePattern { get; set; } = "yyyy-MM-dd_HH-mm-ss";

    public bool OpenAfterCreate { get; set; } = true;

    public bool StartWithWindows { get; set; }

    public int MaxRecentFiles { get; set; } = 10;
}

public sealed class HotkeyConfig
{
    public List<string> Modifiers { get; set; } = ["Ctrl", "Alt"];

    public string Key { get; set; } = "N";

    public override string ToString()
    {
        var parts = Modifiers
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToList();

        if (!string.IsNullOrWhiteSpace(Key))
        {
            parts.Add(Key.Trim().ToUpperInvariant());
        }

        return string.Join(" + ", parts);
    }
}
