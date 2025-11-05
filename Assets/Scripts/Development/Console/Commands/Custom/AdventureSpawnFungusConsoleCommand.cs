using System.Collections.Generic;

public sealed class AdventureSpawnFungusConsoleCommand : IDeveloperConsoleCommand
{
    private static readonly string[] s_aliases =
    {
        "adv.spawn_fungus",
        "adventure.create_fungus"
    };

    public string Key => "adventure.spawn_fungus";
    public string Description => "Создает грибы (юниты) на adventure-сцене рядом с игроком.";
    public string Usage => "adventure.spawn_fungus <unitType> [count]";
    public IReadOnlyList<string> Aliases => s_aliases;

    public void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output)
    {
        if (context.AdventureCommander == null)
        {
            output.AppendError("Команда доступна только в Adventure-сцене.");
            return;
        }

        if (args == null || args.Count < 1)
        {
            output.AppendError($"Использование: {Usage}");
            return;
        }

        if (!DeveloperConsoleParsing.TryParseEnum<UnitType>(args[0], out var unitType))
        {
            output.AppendError($"Неизвестный тип гриба '{args[0]}'.");
            return;
        }

        var count = 1;
        if (args.Count >= 2 && !DeveloperConsoleParsing.TryParseInt(args[1], out count, minValue: 1))
        {
            output.AppendError($"Некорректное количество '{args[1]}'.");
            return;
        }

        context.AdventureCommander.SpawnFungus(unitType, count, output);
    }
}
