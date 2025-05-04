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
    private readonly ILogger _innerLogger;
    private readonly string _prefix;

    public PrefixLogger(ILogger innerLogger, string prefix)
    {
        _innerLogger = innerLogger;
        _prefix = prefix;
    }

    public void Log(string message) => _innerLogger.Log($"{_prefix} {message}");
    public void LogWarning(string message) => _innerLogger.LogWarning($"{_prefix} {message}");
    public void LogError(string message) => _innerLogger.LogError($"{_prefix} {message}");
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
    private readonly string _filePath;

    /// <summary>
    /// ファイルにログを書くよう
    /// </summary>
    /// <param name="filePath"></param>
    public FileLogger(string filePath)
    {
        _filePath = filePath;
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
    }

    private void AppendToFile(string message)
    {
        File.AppendAllText(_filePath, message + Environment.NewLine);
    }

    public void Log(string message) => AppendToFile($"[INFO] {message}");
    public void LogWarning(string message) => AppendToFile($"[WARN] {message}");
    public void LogError(string message) => AppendToFile($"[ERROR] {message}");
}
