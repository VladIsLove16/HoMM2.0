using System.Collections.Generic;

public sealed class GridSpawnFungusConsoleCommand : IDeveloperConsoleCommand
{
    private static readonly string[] s_aliases =
    {
        "grid.create_fungus"
    };

    public string Key => "grid.spawn_fungus";
    public string Description => "Создает гриб на боевой сетке по указанным координатам.";
    public string Usage => "grid.spawn_fungus <x> <y> <unitType>";
    public IReadOnlyList<string> Aliases => s_aliases;

    public void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output)
    {
        if (context.GridCommander == null)
        {
            output.AppendError("Команда доступна только на сеточной сцене.");
            return;
        }

        if (args == null || args.Count < 3)
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

        if (!DeveloperConsoleParsing.TryParseEnum<UnitType>(args[2], out var unitType))
        {
            output.AppendError($"Неизвестный тип гриба '{args[2]}'.");
            return;
        }

        context.GridCommander.SpawnFungus(x, y, unitType, output);
    }
}
