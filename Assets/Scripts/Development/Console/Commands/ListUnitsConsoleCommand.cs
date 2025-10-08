using System.Collections.Generic;
using System.Text;

public class ListUnitsConsoleCommand : IDeveloperConsoleCommand
{
    private static readonly string[] _aliases = { "units", "ls_units" };

    public string Key => "list_units";
    public string Description => "Lists all units currently present on the grid.";
    public string Usage => "list_units";
    public IReadOnlyList<string> Aliases => _aliases;

    public void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output)
    {
        if (context.GameModel == null)
        {
            output.AppendError("Game model is not available.");
            return;
        }

        var units = context.GameModel.GetUnits();
        if (units == null || units.Count == 0)
        {
            output.AppendWarning("No units present on the grid.");
            return;
        }

        var builder = new StringBuilder();
        builder.AppendLine($"Units ({units.Count}):");
        foreach (var unit in units)
        {
            builder.Append(" - ")
                   .Append(unit.UnitType.Value)
                   .Append(" [")
                   .Append(unit.Team.Value)
                   .Append("] at (")
                   .Append(unit.Position.Value.x)
                   .Append(',')
                   .Append(unit.Position.Value.y)
                   .Append(") amount=")
                   .Append(unit.Amount.Value)
                   .Append(" HP=")
                   .Append(unit.ModifiedStats.Health)
                   .Append('/')
                   .Append(unit.ModifiedStats.MaxHealth);

            if (!unit.CanMove.Value)
            {
                builder.Append(" cannot-move");
            }
            if (!unit.CanAttack.Value)
            {
                builder.Append(" cannot-attack");
            }

            builder.AppendLine();
        }

        output.AppendLine(builder.ToString().TrimEnd());
    }
}
