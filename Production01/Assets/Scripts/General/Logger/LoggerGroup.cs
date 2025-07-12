using System;
using System.IO;
using UnityEngine;
public class UnityLogger : ILogger
{
    public void Log(string message) => Debug.Log(message);
    public void LogWarning(string message) => Debug.LogWarning(message);
    public void LogError(string message) => Debug.LogError(message);
}

/// <summary>
/// 「何の」エラーかって出したいとき
/// </summary>
public class PrefixLogger : ILogger
{
    private readonly ILogger _InnerLogger;
    private readonly string _Prefix;

    public PrefixLogger(ILogger innerLogger, string prefix)
    {
        _InnerLogger = innerLogger;
        _Prefix = prefix;
    }

    public void Log(string message) => _InnerLogger.Log($"{_Prefix} {message}");
    public void LogWarning(string message) => _InnerLogger.LogWarning($"{_Prefix} {message}");
    public void LogError(string message) => _InnerLogger.LogError($"{_Prefix} {message}");
}

/// <summary>
/// 後で消すかも
/// 何も出さなくていいとき
/// </summary>
public class SilentLogger : ILogger
{
    public void Log(string message) { }
    public void LogWarning(string message) { }
    public void LogError(string message) { }
}

public class FileLogger : ILogger
{
    private readonly string _FilePath;

    /// <summary>
    /// ファイルにログを書くよう
    /// </summary>
    /// <param name="filePath"></param>
    public FileLogger(string filePath)
    {
        _FilePath = filePath;
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
    }

    private void AppendToFile(string message)
    {
        File.AppendAllText(_FilePath, message + Environment.NewLine);
    }

    public void Log(string message) => AppendToFile($"[INFO] {message}");
    public void LogWarning(string message) => AppendToFile($"[WARN] {message}");
    public void LogError(string message) => AppendToFile($"[ERROR] {message}");
}
