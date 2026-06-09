namespace QuickTxt;

public sealed class QuickTxtApplicationContext : ApplicationContext
{
    private readonly ConfigService _configService = new();
    private readonly NotifyIcon _notifyIcon;
    private readonly HotkeyService _hotkeyService;

    private AppConfig _config;
    private QuickTxtService _quickTxtService;
    private SettingsForm? _settingsForm;

    public QuickTxtApplicationContext()
    {
        _config = _configService.Load();
        _quickTxtService = new QuickTxtService(_config);
        ApplyStartupSetting(showError: false);

        _notifyIcon = new NotifyIcon
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application,
            Text = "QuickTxt",
            Visible = true
        };

        _notifyIcon.ContextMenuStrip = BuildContextMenu();
        _notifyIcon.DoubleClick += (_, _) => CreateAndOpenTextFile();

        _hotkeyService = new HotkeyService();
        _hotkeyService.HotkeyPressed += (_, _) => CreateAndOpenTextFile();

        RegisterHotkey(showSuccess: false);
    }

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Opening += (_, _) => RebuildRecentMenu(menu);

        menu.Items.Add("新建 txt", null, (_, _) => CreateAndOpenTextFile());
        menu.Items.Add("删除最近空 txt", null, (_, _) => DeleteEmptyRecentTextFiles());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(CreateRecentMenuItem());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("打开保存目录", null, (_, _) => OpenSaveDirectory());
        menu.Items.Add("设置", null, (_, _) => ShowSettings());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出", null, (_, _) => ExitApplication());

        return menu;
    }

    private ToolStripMenuItem CreateRecentMenuItem()
    {
        var item = new ToolStripMenuItem("最近 txt");
        item.DropDownItems.Add("暂无最近文件").Enabled = false;
        return item;
    }

    private void RebuildRecentMenu(ContextMenuStrip menu)
    {
        var recentMenu = menu.Items
            .OfType<ToolStripMenuItem>()
            .FirstOrDefault(item => item.Text == "最近 txt");

        if (recentMenu is null)
        {
            return;
        }

        recentMenu.DropDownItems.Clear();

        try
        {
            var files = _quickTxtService.GetRecentTextFiles();
            if (files.Count == 0)
            {
                recentMenu.DropDownItems.Add("暂无最近文件").Enabled = false;
                return;
            }

            foreach (var file in files)
            {
                var item = recentMenu.DropDownItems.Add(Path.GetFileName(file));
                item.ToolTipText = file;
                item.Click += (_, _) => OpenRecentFile(file);
            }
        }
        catch (Exception ex)
        {
            recentMenu.DropDownItems.Add($"读取失败：{ex.Message}").Enabled = false;
        }
    }

    private void CreateAndOpenTextFile()
    {
        try
        {
            var path = _quickTxtService.CreateTextFile();
            _notifyIcon.ShowBalloonTip(
                1500,
                "QuickTxt",
                $"已创建：{Path.GetFileName(path)}",
                ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            ShowError("创建 txt 失败", ex);
        }
    }

    private void OpenRecentFile(string path)
    {
        try
        {
            _quickTxtService.OpenFile(path);
        }
        catch (Exception ex)
        {
            ShowError("打开最近文件失败", ex);
        }
    }

    private void OpenSaveDirectory()
    {
        try
        {
            _quickTxtService.OpenSaveDirectory();
        }
        catch (Exception ex)
        {
            ShowError("打开保存目录失败", ex);
        }
    }

    private void DeleteEmptyRecentTextFiles()
    {
        var confirm = MessageBox.Show(
            "将检查保存目录下最近 10 个 txt 文件，并删除其中 0 字节的空文件。\n\n是否继续？",
            "QuickTxt",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            var deletedFiles = _quickTxtService.DeleteEmptyRecentTextFiles(limit: 10);
            var message = deletedFiles.Count == 0
                ? "最近 10 个 txt 中没有空文件。"
                : $"已删除 {deletedFiles.Count} 个空 txt。";

            _notifyIcon.ShowBalloonTip(2500, "QuickTxt", message, ToolTipIcon.Info);
            MessageBox.Show(message, "QuickTxt", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            ShowError("删除空 txt 失败", ex);
        }
    }

    private void ShowSettings()
    {
        if (_settingsForm is { IsDisposed: false })
        {
            _settingsForm.Activate();
            return;
        }

        _settingsForm = new SettingsForm(_config);
        _settingsForm.SettingsSaved += (_, newConfig) =>
        {
            _config = newConfig;
            _configService.Save(_config);
            _quickTxtService = new QuickTxtService(_config);
            ApplyStartupSetting(showError: true);
            RegisterHotkey(showSuccess: true);
        };
        _settingsForm.Show();
        _settingsForm.Activate();
    }

    private void RegisterHotkey(bool showSuccess)
    {
        if (_hotkeyService.Register(_config.Hotkey))
        {
            if (showSuccess)
            {
                _notifyIcon.ShowBalloonTip(1500, "QuickTxt", $"快捷键已更新：{_config.Hotkey}", ToolTipIcon.Info);
            }

            return;
        }

        MessageBox.Show(
            $"全局快捷键注册失败：{_config.Hotkey}\n\n可能已被其他程序占用，请在设置中更换快捷键。",
            "QuickTxt",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    private void ApplyStartupSetting(bool showError)
    {
        try
        {
            StartupService.Apply(_config.StartWithWindows);
        }
        catch (Exception ex)
        {
            if (showError)
            {
                ShowError("更新开机自启动失败", ex);
            }
        }
    }

    private void ShowError(string title, Exception ex)
    {
        _notifyIcon.ShowBalloonTip(3000, "QuickTxt", $"{title}：{ex.Message}", ToolTipIcon.Error);
        MessageBox.Show(ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void ExitApplication()
    {
        _settingsForm?.Close();
        _hotkeyService.Dispose();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        ExitThread();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hotkeyService.Dispose();
            _notifyIcon.Dispose();
            _settingsForm?.Dispose();
        }

        base.Dispose(disposing);
    }
}
