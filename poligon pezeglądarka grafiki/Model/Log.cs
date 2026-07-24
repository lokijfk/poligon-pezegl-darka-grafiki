using System.IO;
using System;

namespace poligon_pezeglądarka_grafiki.Model;

/// <summary>
/// Reprezentuje poziomy logowania.
/// </summary>
internal enum LogLevel
{
    Trace,
    Debug,
    Info,
    Warning,
    Error,
    Critical,
    None
}

/// <summary>
/// Represents a logging utility for writing log messages to a file.
/// </summary>
internal class Log
{
    // do uzupełnienia całość
    public static object? LogLevel { get; private set; }

    /// <summary>
    /// Writes a log message to a file. If no path is provided, the log file will be created in the current directory with a name based on the current date.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="path"></param>
    static public void Write(string message, string path)
    {
        string logFilePath = string.IsNullOrEmpty(path) ? "log - " + DateTime.Now.ToString("yyyy-MM-dd") + ".txt" : path+ "\\log - " + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
        string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            Directory.CreateDirectory(path);
        File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
    }

    internal static void Write(LogLevel info, string v, string path = "")
    {
        // zapisz poziom w komunikacie i przekaż dalej
        Write($"[{info}]:  {v}", path);
    }

    static public void Write(LogLevel logLevel, string message)
    {
        // Implement logging with log level if needed        
        Write($"[{logLevel}]:  {message}");
    }

    static public void Write(string message)
    {
        // Implement logging with log level if needed
        Write(message, BrokerFile.GetUserAppDataPath + "\\Log");
    }
}
