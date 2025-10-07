using System.Collections.Generic;

public class SpawnUnitConsoleCommand : IDeveloperConsoleCommand
{
    private static readonly string[] _aliases = { "spawn_unit", "spawnunit" };

    public string Key => "spawn";
    public string Description => "Spawns a unit at the given grid coordinates.";
    public string Usage => "spawn <unitType> <team> <x> <y> [amount]";
    public IReadOnlyList<string> Aliases => _aliases;

    public void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output)
    {
        if (context.GameModel == null)
        {
            output.AppendError("Game model is not available.");
            return;
        }

        if (args == null || args.Count < 4)
        {
            output.AppendError($"Usage: {Usage}");
            return;
        }

        if (!DeveloperConsoleParsing.TryParseEnum<UnitType>(args[0], out var unitType))
        {
            output.AppendError($"Unknown unit type '{args[0]}'.");
            return;
        }

        if (!DeveloperConsoleParsing.TryParseEnum<Team>(args[1], out var team))
        {
            output.AppendError($"Unknown team '{args[1]}'.");
            return;
        }

        if (!DeveloperConsoleParsing.TryParseInt(args[2], out var x))
        {
            output.AppendError($"Invalid X coordinate '{args[2]}'.");
            return;
        }

        if (!DeveloperConsoleParsing.TryParseInt(args[3], out var y))
        {
            output.AppendError($"Invalid Y coordinate '{args[3]}'.");
            return;
        }

        int amount = 1;
        if (args.Count >= 5)
        {
            if (!DeveloperConsoleParsing.TryParseInt(args[4], out amount, minValue: 1))
            {
                output.AppendError($"Invalid amount '{args[4]}'. Amount must be >= 1.");
                return;
            }
        }

        var result = context.GameModel.SpawnUnit(new UnitSpawnParams(x, y, unitType, amount, team));
        if (!result.IsSuccess)
        {
            output.AppendError(string.IsNullOrEmpty(result.Message)
                ? "Spawn failed."
                : $"Spawn failed: {result.Message}");
            return;
        }

        output.AppendLine($"Spawned {unitType} ({team}) at ({x},{y}) with amount {amount}.");
    }
}
