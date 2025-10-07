using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class InspectCellConsoleCommand : IDeveloperConsoleCommand
{
    private static readonly string[] _aliases = { "cell", "inspect" };

    public string Key => "inspect_cell";
    public string Description => "Shows detailed information about a grid cell.";
    public string Usage => "inspect_cell <x> <y>";
    public IReadOnlyList<string> Aliases => _aliases;

    public void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output)
    {
        if (context.GameModel == null)
        {
            output.AppendError("Game model is not available.");
            return;
        }

        if (args == null || args.Count < 2)
        {
            output.AppendError($"Usage: {Usage}");
            return;
        }

        if (!DeveloperConsoleParsing.TryParseVector2Int(args, 0, out var coords))
        {
            output.AppendError("Invalid coordinates. Expected two integers.");
            return;
        }

        if (!context.GameModel.IsInBounds(coords))
        {
            output.AppendError($"Cell ({coords.x},{coords.y}) is outside the grid bounds.");
            return;
        }

        var cellInterface = context.GameModel.GetCell(coords);
        if (cellInterface == null)
        {
            output.AppendError($"Cell ({coords.x},{coords.y}) could not be resolved.");
            return;
        }

        if (cellInterface is not GameCell cell)
        {
            output.AppendError("Grid cell type is not supported by the console inspector.");
            return;
        }

        var builder = new StringBuilder();
        builder.AppendLine($"Cell ({cell.X},{cell.Y}):");
        builder.AppendLine(cell.IsEmpty ? " - empty" : " - contents:");

        foreach (var content in cell.Contents)
        {
            builder.Append("   * ")
                   .Append(content.GridContentType)
                   .Append(' ')
                   .Append(content.ToString());

            if (content is UnitModel unit)
            {
                builder.Append(" Team=")
                       .Append(unit.Team.Value)
                       .Append(" Amount=")
                       .Append(unit.Amount.Value)
                       .Append(" HP=")
                       .Append(unit.ModifiedStats.Health)
                       .Append('/')
                       .Append(unit.ModifiedStats.MaxHealth);
            }

            builder.AppendLine();
        }

        output.AppendLine(builder.ToString().TrimEnd());
    }
}
