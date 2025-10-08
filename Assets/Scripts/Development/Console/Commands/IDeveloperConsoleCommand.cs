using System.Collections.Generic;

public interface IDeveloperConsoleCommand
{
    string Key { get; }
    string Description { get; }
    string Usage { get; }
    IReadOnlyList<string> Aliases { get; }
    void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output);
}
