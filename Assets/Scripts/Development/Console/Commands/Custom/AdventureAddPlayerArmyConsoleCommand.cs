using System.Collections.Generic;

public sealed class AdventureAddPlayerArmyConsoleCommand : IDeveloperConsoleCommand
{
    private static readonly string[] s_aliases =
    {
        "adv.add_player_army"
    };

    public string Key => "adventure.add_player_army";
    public string Description => "Добавляет указанный тип юнитов в армию игрока.";
    public string Usage => "adventure.add_player_army <unitType> <count>";
    public IReadOnlyList<string> Aliases => s_aliases;

    public void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output)
    {
        if (context.AdventureCommander == null)
        {
            output.AppendError("Команда доступна только в Adventure-сцене.");
            return;
        }

        if (args == null || args.Count < 2)
        {
            output.AppendError($"Использование: {Usage}");
            return;
        }

        if (!DeveloperConsoleParsing.TryParseEnum<UnitType>(args[0], out var unitType))
        {
            output.AppendError($"Неизвестный тип юнита '{args[0]}'.");
            return;
        }

        if (!DeveloperConsoleParsing.TryParseInt(args[1], out var count, minValue: 1))
        {
            output.AppendError($"Некорректное количество '{args[1]}'.");
            return;
        }

        context.AdventureCommander.AddArmyToPlayer(unitType, count, output);
    }
}
