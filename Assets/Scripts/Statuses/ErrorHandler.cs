using System;
using System.IO;
using UnityEngine;

public class ErrorHandler : MonoBehaviour
{
    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            // Log or display the error details
            SaveLogInfo($"Exception: {logString}\nStackTrace: {stackTrace}");
        }
    }
    private void SaveLogInfo(string text)
    {
        string currentDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string logFileName = $"Log_{currentDate}.txt";
        string logFilePath = Path.Combine(Application.streamingAssetsPath, "logs", logFileName);

        CreateDirectoryIfNotExists(Path.GetDirectoryName(logFilePath));
        LogMessage(text, logFilePath);
    }

    private void CreateDirectoryIfNotExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    public void LogMessage(string message,string path)
    {
        using (StreamWriter writer = File.AppendText(path))
        {
            writer.WriteLine($"{DateTime.Now}: {message}");
        }
    }
}
