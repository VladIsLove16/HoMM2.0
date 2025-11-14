using System.Collections.Generic;
using UnityEngine;

public class SelectCellConsoleCommand : IDeveloperConsoleCommand
{
    private static readonly string[] _aliases = { "select_cell", "act" };

    public string Key => "select";
    public string Description => "Simulates selecting a grid cell, triggering the current unit action.";
    public string Usage => "select <x> <y> [attackFromX attackFromY]";
    public IReadOnlyList<string> Aliases => _aliases;

    public void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output)
    {
        if (context.GridViewModel == null)
        {
            output.AppendError("Game view model is not available.");
            return;
        }

        if (args == null || args.Count < 2)
        {
            output.AppendError($"Usage: {Usage}");
            return;
        }

        if (!DeveloperConsoleParsing.TryParseVector2Int(args, 0, out var target))
        {
            output.AppendError("Invalid target coordinates. Expected integers.");
            return;
        }

        Vector2Int attackFrom = target;
        if (args.Count >= 4)
        {
            if (!DeveloperConsoleParsing.TryParseVector2Int(args, 2, out attackFrom))
            {
                output.AppendError("Invalid attack-from coordinates. Expected integers.");
                return;
            }
        }

        var pair = new KeyValuePair<Vector2Int, Vector2Int>(target, attackFrom);
        context.GridViewModel.HandleCellSelected(pair);
        output.AppendLine($"Selected cell ({target.x},{target.y}) from ({attackFrom.x},{attackFrom.y}).");
    }
}
