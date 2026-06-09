# QuickTxt

QuickTxt 是一个 Windows 托盘常驻小工具。按下全局快捷键后，它会在配置目录中按当前时间新建一个 txt 文件，并立即用系统默认编辑器打开。

## 当前功能

- 后台托盘常驻。
- 默认全局快捷键：`Ctrl + Alt + N`。
- 按当前时间新建 txt，例如 `2026-06-09_14-32-08.txt`。
- 保存目录可配置。
- 创建后自动用默认 txt 编辑器打开。
- 支持登录 Windows 后自动启动。
- 托盘菜单支持：
  - 新建 txt
  - 删除最近空 txt
  - 最近 txt
  - 打开保存目录
  - 设置
  - 退出
- 配置文件保存到：

```text
%AppData%\QuickTxt\config.json
```

## 项目结构

```text
.
├─ QuickTxt
│  ├─ Assets
│  │  ├─ QuickTxt.ico
│  │  └─ QuickTxt.png
│  ├─ Tools
│  │  └─ GenerateIcon.ps1
│  ├─ QuickTxt.csproj
│  ├─ Program.cs
│  ├─ QuickTxtApplicationContext.cs
│  ├─ SettingsForm.cs
│  ├─ AppConfig.cs
│  ├─ ConfigService.cs
│  ├─ QuickTxtService.cs
│  ├─ HotkeyService.cs
│  ├─ StartupService.cs
│  ├─ GlobalUsings.cs
│  └─ app.manifest
├─ QuickTxtDesign
│  └─ FUNCTIONAL_DESIGN.md
├─ .gitignore
└─ README.md
```

## 开发环境

需要安装：

- Visual Studio Community，安装时勾选 `.NET 桌面开发`
- 或单独安装 .NET SDK 8 及以上版本

当前项目目标框架：

```text
net8.0-windows
```

## 运行

在项目目录执行：

```powershell
cd QuickTxt
dotnet run
```

启动后程序会进入系统托盘，不会显示主窗口。

## 发布 exe

发布为 Windows x64 单文件 exe：

```powershell
cd QuickTxt
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

发布结果通常位于：

```text
QuickTxt\bin\Release\net8.0-windows\win-x64\publish\QuickTxt.exe
```

## 默认配置

首次启动时会自动生成配置：

```json
{
  "saveDirectory": "%USERPROFILE%\\Documents\\QuickTxt",
  "hotkey": {
    "modifiers": ["Ctrl", "Alt"],
    "key": "N"
  },
  "fileNamePattern": "yyyy-MM-dd_HH-mm-ss",
  "openAfterCreate": true,
  "startWithWindows": false,
  "maxRecentFiles": 10
}
```

## 开机自启动

在设置窗口中勾选 `登录 Windows 后自动启动 QuickTxt` 后，程序会写入当前用户的 Windows 启动项：

```text
HKCU\Software\Microsoft\Windows\CurrentVersion\Run
```

这个方式只对当前用户生效，不需要管理员权限。

## 删除最近空 txt

托盘菜单中的 `删除最近空 txt` 会检查保存目录下最近修改的 10 个 `.txt` 文件，并删除其中 0 字节的空文件。

这个功能不会扫描整个目录，也不会删除包含空格、换行或其他字符的文件。

## 后续可扩展

- 自定义默认编辑器。
- 最近文件使用 `recent.json` 精确记录。
- 更换托盘图标。
- 增加安装包。
- 增加代码签名。
