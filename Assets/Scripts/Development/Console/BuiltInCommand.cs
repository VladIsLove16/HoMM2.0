using System;
using System.Collections.Generic;

public partial class DeveloperConsoleView
{
    private sealed class BuiltInCommand : IDeveloperConsoleCommand
    {
        public BuiltInCommand(string key, string description, string usage)
        {
            Key = key;
            Description = description;
            Usage = usage;
            Aliases = Array.Empty<string>();
        }

        public string Key { get; }
        public string Description { get; }
        public string Usage { get; }
        public IReadOnlyList<string> Aliases { get; }

        public void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output)
        {
            throw new NotSupportedException("Built-in commands are handled directly by DeveloperConsoleService.");
        }
    }
}


