using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace QuickTxt;

public sealed class QuickTxtService
{
    private readonly AppConfig _config;

    public QuickTxtService(AppConfig config)
    {
        _config = config;
    }

    public string CreateTextFile()
    {
        Directory.CreateDirectory(_config.SaveDirectory);

        var baseName = DateTime.Now.ToString(_config.FileNamePattern, CultureInfo.InvariantCulture);
        baseName = SanitizeFileName(baseName);
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
        }

        var path = GetAvailableFilePath(baseName);
        File.WriteAllText(path, string.Empty, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        if (_config.OpenAfterCreate)
        {
            OpenFile(path);
        }

        return path;
    }

    public IReadOnlyList<string> GetRecentTextFiles()
    {
        return GetRecentTextFiles(_config.MaxRecentFiles);
    }

    public IReadOnlyList<string> GetRecentTextFiles(int limit)
    {
        if (!Directory.Exists(_config.SaveDirectory))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(_config.SaveDirectory, "*.txt", SearchOption.TopDirectoryOnly)
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.LastWriteTime)
            .Take(Math.Max(1, limit))
            .Select(file => file.FullName)
            .ToList();
    }

    public IReadOnlyList<string> DeleteEmptyRecentTextFiles(int limit)
    {
        var deletedFiles = new List<string>();

        foreach (var path in GetRecentTextFiles(limit))
        {
            var file = new FileInfo(path);
            if (!file.Exists || file.Length > 0)
            {
                continue;
            }

            file.Delete();
            deletedFiles.Add(path);
        }

        return deletedFiles;
    }

    public void OpenFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("文件不存在。", path);
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    public void OpenSaveDirectory()
    {
        Directory.CreateDirectory(_config.SaveDirectory);

        Process.Start(new ProcessStartInfo
        {
            FileName = _config.SaveDirectory,
            UseShellExecute = true
        });
    }

    private string GetAvailableFilePath(string baseName)
    {
        var path = Path.Combine(_config.SaveDirectory, $"{baseName}.txt");
        if (!File.Exists(path))
        {
            return path;
        }

        for (var index = 2; index < 10_000; index++)
        {
            path = Path.Combine(_config.SaveDirectory, $"{baseName}_{index}.txt");
            if (!File.Exists(path))
            {
                return path;
            }
        }

        throw new IOException("无法生成不重复的 txt 文件名。");
    }

    private static string SanitizeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);

        foreach (var ch in value)
        {
            builder.Append(invalidChars.Contains(ch) ? '-' : ch);
        }

        return builder.ToString().Trim(' ', '.');
    }
}
