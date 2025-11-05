using System.Collections.Generic;

public sealed class GridKillFungusConsoleCommand : IDeveloperConsoleCommand
{
    private static readonly string[] s_aliases =
    {
        "grid.remove_fungus"
    };

    public string Key => "grid.kill_fungus";
    public string Description => "Удаляет гриб (юнита) из указанной клетки боевой сетки.";
    public string Usage => "grid.kill_fungus <x> <y>";
    public IReadOnlyList<string> Aliases => s_aliases;

    public void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output)
    {
        if (context.GridCommander == null)
        {
            output.AppendError("Команда доступна только на сеточной сцене.");
            return;
        }

        if (args == null || args.Count < 2)
        {
            output.AppendError($"Использование: {Usage}");
            return;
        }

        if (!DeveloperConsoleParsing.TryParseInt(args[0], out var x))
        {
            output.AppendError($"Некорректная координата X '{args[0]}'.");
            return;
        }

        if (!DeveloperConsoleParsing.TryParseInt(args[1], out var y))
        {
            output.AppendError($"Некорректная координата Y '{args[1]}'.");
            return;
        }

        context.GridCommander.KillFungus(x, y, output);
    }
}
