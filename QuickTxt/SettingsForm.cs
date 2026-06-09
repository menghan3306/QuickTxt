namespace QuickTxt;

public sealed class SettingsForm : Form
{
    private readonly TextBox _saveDirectoryTextBox;
    private readonly CheckBox _ctrlCheckBox;
    private readonly CheckBox _altCheckBox;
    private readonly CheckBox _shiftCheckBox;
    private readonly CheckBox _winCheckBox;
    private readonly ComboBox _keyComboBox;
    private readonly TextBox _fileNamePatternTextBox;
    private readonly NumericUpDown _maxRecentFilesInput;
    private readonly CheckBox _openAfterCreateCheckBox;
    private readonly CheckBox _startWithWindowsCheckBox;

    public event EventHandler<AppConfig>? SettingsSaved;

    public SettingsForm(AppConfig config)
    {
        Text = "QuickTxt 设置";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        ShowInTaskbar = true;
        ClientSize = new Size(560, 360);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 3,
            RowCount = 8
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));

        for (var i = 0; i < 6; i++)
        {
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        }
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

        Controls.Add(root);

        _saveDirectoryTextBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
        var browseButton = new Button { Text = "选择", Anchor = AnchorStyles.Left | AnchorStyles.Right };
        browseButton.Click += (_, _) => BrowseSaveDirectory();
        AddLabel(root, "保存目录", 0);
        root.Controls.Add(_saveDirectoryTextBox, 1, 0);
        root.Controls.Add(browseButton, 2, 0);

        var hotkeyPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        _ctrlCheckBox = new CheckBox { Text = "Ctrl", AutoSize = true };
        _altCheckBox = new CheckBox { Text = "Alt", AutoSize = true };
        _shiftCheckBox = new CheckBox { Text = "Shift", AutoSize = true };
        _winCheckBox = new CheckBox { Text = "Win", AutoSize = true };
        _keyComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 70
        };
        _keyComboBox.Items.AddRange(GetSupportedKeys());
        hotkeyPanel.Controls.AddRange([_ctrlCheckBox, _altCheckBox, _shiftCheckBox, _winCheckBox, _keyComboBox]);
        AddLabel(root, "快捷键", 1);
        root.Controls.Add(hotkeyPanel, 1, 1);
        root.SetColumnSpan(hotkeyPanel, 2);

        _fileNamePatternTextBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
        AddLabel(root, "命名格式", 2);
        root.Controls.Add(_fileNamePatternTextBox, 1, 2);
        root.SetColumnSpan(_fileNamePatternTextBox, 2);

        _maxRecentFilesInput = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 50,
            Anchor = AnchorStyles.Left,
            Width = 80
        };
        AddLabel(root, "最近数量", 3);
        root.Controls.Add(_maxRecentFilesInput, 1, 3);
        root.SetColumnSpan(_maxRecentFilesInput, 2);

        _openAfterCreateCheckBox = new CheckBox { Text = "创建后自动打开 txt", AutoSize = true };
        AddLabel(root, "打开方式", 4);
        root.Controls.Add(_openAfterCreateCheckBox, 1, 4);
        root.SetColumnSpan(_openAfterCreateCheckBox, 2);

        _startWithWindowsCheckBox = new CheckBox { Text = "登录 Windows 后自动启动 QuickTxt", AutoSize = true };
        AddLabel(root, "启动项", 5);
        root.Controls.Add(_startWithWindowsCheckBox, 1, 5);
        root.SetColumnSpan(_startWithWindowsCheckBox, 2);

        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 6, 0, 0),
            WrapContents = false
        };
        var saveButton = new Button { Text = "保存", Width = 86, DialogResult = DialogResult.None };
        var cancelButton = new Button { Text = "取消", Width = 86 };
        saveButton.Click += (_, _) => SaveSettings();
        cancelButton.Click += (_, _) => Close();
        buttonsPanel.Controls.Add(saveButton);
        buttonsPanel.Controls.Add(cancelButton);
        root.Controls.Add(buttonsPanel, 1, 7);
        root.SetColumnSpan(buttonsPanel, 2);

        AcceptButton = saveButton;
        CancelButton = cancelButton;

        LoadConfig(config);
    }

    private void LoadConfig(AppConfig config)
    {
        _saveDirectoryTextBox.Text = config.SaveDirectory;
        _fileNamePatternTextBox.Text = config.FileNamePattern;
        _maxRecentFilesInput.Value = Math.Clamp(config.MaxRecentFiles, 1, 50);
        _openAfterCreateCheckBox.Checked = config.OpenAfterCreate;
        _startWithWindowsCheckBox.Checked = config.StartWithWindows || StartupService.IsEnabled();

        var modifiers = config.Hotkey.Modifiers.Select(value => value.ToUpperInvariant()).ToHashSet();
        _ctrlCheckBox.Checked = modifiers.Contains("CTRL") || modifiers.Contains("CONTROL");
        _altCheckBox.Checked = modifiers.Contains("ALT");
        _shiftCheckBox.Checked = modifiers.Contains("SHIFT");
        _winCheckBox.Checked = modifiers.Contains("WIN") || modifiers.Contains("WINDOWS");

        var key = string.IsNullOrWhiteSpace(config.Hotkey.Key)
            ? "N"
            : config.Hotkey.Key.ToUpperInvariant();
        _keyComboBox.SelectedItem = _keyComboBox.Items.Contains(key) ? key : "N";
    }

    private void SaveSettings()
    {
        var directory = _saveDirectoryTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(directory))
        {
            MessageBox.Show("请选择保存目录。", "QuickTxt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var modifiers = new List<string>();
        if (_ctrlCheckBox.Checked)
        {
            modifiers.Add("Ctrl");
        }
        if (_altCheckBox.Checked)
        {
            modifiers.Add("Alt");
        }
        if (_shiftCheckBox.Checked)
        {
            modifiers.Add("Shift");
        }
        if (_winCheckBox.Checked)
        {
            modifiers.Add("Win");
        }

        if (modifiers.Count == 0)
        {
            MessageBox.Show("快捷键至少需要选择一个修饰键。", "QuickTxt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var pattern = _fileNamePatternTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(pattern))
        {
            MessageBox.Show("请填写文件命名格式。", "QuickTxt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _ = DateTime.Now.ToString(pattern);
        }
        catch (FormatException ex)
        {
            MessageBox.Show($"文件命名格式无效：{ex.Message}", "QuickTxt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var config = new AppConfig
        {
            SaveDirectory = directory,
            Hotkey = new HotkeyConfig
            {
                Modifiers = modifiers,
                Key = _keyComboBox.SelectedItem?.ToString() ?? "N"
            },
            FileNamePattern = pattern,
            OpenAfterCreate = _openAfterCreateCheckBox.Checked,
            StartWithWindows = _startWithWindowsCheckBox.Checked,
            MaxRecentFiles = (int)_maxRecentFilesInput.Value
        };

        SettingsSaved?.Invoke(this, config);
        Close();
    }

    private void BrowseSaveDirectory()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "选择 QuickTxt 保存 txt 的目录",
            UseDescriptionForTitle = true,
            SelectedPath = Directory.Exists(_saveDirectoryTextBox.Text)
                ? _saveDirectoryTextBox.Text
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _saveDirectoryTextBox.Text = dialog.SelectedPath;
        }
    }

    private static void AddLabel(TableLayoutPanel root, string text, int row)
    {
        root.Controls.Add(
            new Label
            {
                Text = text,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                TextAlign = ContentAlignment.MiddleLeft
            },
            0,
            row);
    }

    private static object[] GetSupportedKeys()
    {
        var letters = Enumerable.Range('A', 26).Select(value => ((char)value).ToString());
        var numbers = Enumerable.Range(0, 10).Select(value => value.ToString());
        return letters.Concat(numbers).Cast<object>().ToArray();
    }
}
