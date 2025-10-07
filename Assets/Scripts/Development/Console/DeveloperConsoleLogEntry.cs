using System;

public enum DeveloperConsoleLogType
{
    Info,
    Warning,
    Error
}

public readonly struct DeveloperConsoleLogEntry
{
    public DateTime Timestamp { get; }
    public string Message { get; }
    public DeveloperConsoleLogType Type { get; }

    public DeveloperConsoleLogEntry(string message, DeveloperConsoleLogType type)
    {
        Timestamp = DateTime.UtcNow;
        Message = message ?? string.Empty;
        Type = type;
    }

    public override string ToString()
    {
        return $"[{Timestamp:HH:mm:ss}] {Message}";
    }
}
