
using Core.Abstractions;
using System.IO;
using System;

namespace Core.Notifications;

public sealed class FileSink : INotificationSink
{
    private readonly string _path;

    public FileSink(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                "Шлях до журналу не може бути порожнім.",
                nameof(path));
        }

        _path = Path.GetFullPath(path);
    }

    public void Notify(string message)
    {
        string? directory = Path.GetDirectoryName(_path);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.AppendAllText(
            _path,
            message + Environment.NewLine);
    }
}
