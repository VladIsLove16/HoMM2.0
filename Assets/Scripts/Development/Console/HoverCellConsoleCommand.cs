using System.Collections.Generic;
using UnityEngine;

public class HoverCellConsoleCommand : IDeveloperConsoleCommand
{
    private static readonly string[] _aliases = { "hover_cell", "preview" };

    public string Key => "hover";
    public string Description => "Simulates hovering the pointer over a grid cell to preview actions.";
    public string Usage => "hover <x> <y> [attackFromX attackFromY]";
    public IReadOnlyList<string> Aliases => _aliases;

    public void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output)
    {
        if (context.GameViewModel == null)
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
            output.AppendError("Invalid coordinates. Expected two integers for target cell.");
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
        context.GameViewModel.HandleCellHovered(pair);
        output.AppendLine($"Hovered cell ({target.x},{target.y}) with attack-from ({attackFrom.x},{attackFrom.y}).");
    }
}
