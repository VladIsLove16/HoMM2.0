using System;

public interface IDeveloperConsoleOutput
{
    void AppendLine(string message);
    void AppendWarning(string message);
    void AppendError(string message);
}
