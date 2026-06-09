using System.Runtime.InteropServices;

namespace QuickTxt;

public sealed class HotkeyService : NativeWindow, IDisposable
{
    private const int HotkeyId = 0x5154;
    private const int WmHotkey = 0x0312;

    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint ModWin = 0x0008;
    private const uint ModNoRepeat = 0x4000;

    private bool _registered;

    public event EventHandler? HotkeyPressed;

    public HotkeyService()
    {
        CreateHandle(new CreateParams
        {
            Caption = "QuickTxt Hotkey Window"
        });
    }

    public bool Register(HotkeyConfig config)
    {
        Unregister();

        var modifiers = ParseModifiers(config.Modifiers) | ModNoRepeat;
        var key = ParseKey(config.Key);
        if (key == Keys.None)
        {
            return false;
        }

        _registered = RegisterHotKey(Handle, HotkeyId, modifiers, (uint)key);
        return _registered;
    }

    public void Unregister()
    {
        if (!_registered)
        {
            return;
        }

        UnregisterHotKey(Handle, HotkeyId);
        _registered = false;
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotkey && m.WParam.ToInt32() == HotkeyId)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
            return;
        }

        base.WndProc(ref m);
    }

    public void Dispose()
    {
        Unregister();
        DestroyHandle();
        GC.SuppressFinalize(this);
    }

    private static uint ParseModifiers(IEnumerable<string> modifiers)
    {
        uint result = 0;

        foreach (var modifier in modifiers)
        {
            switch (modifier.Trim().ToUpperInvariant())
            {
                case "CTRL":
                case "CONTROL":
                    result |= ModControl;
                    break;
                case "ALT":
                    result |= ModAlt;
                    break;
                case "SHIFT":
                    result |= ModShift;
                    break;
                case "WIN":
                case "WINDOWS":
                    result |= ModWin;
                    break;
            }
        }

        return result;
    }

    private static Keys ParseKey(string key)
    {
        Keys parsed;

        if (key.Length == 1 && char.IsLetterOrDigit(key[0]))
        {
            if (char.IsDigit(key[0]))
            {
                return Enum.TryParse<Keys>($"D{key[0]}", out parsed) ? parsed : Keys.None;
            }

            return Enum.TryParse<Keys>(key.ToUpperInvariant(), out parsed) ? parsed : Keys.None;
        }

        if (Enum.TryParse<Keys>(key, ignoreCase: true, out parsed))
        {
            return parsed;
        }

        return Keys.None;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
