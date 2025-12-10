using System.Collections.Generic;

public sealed class GridKillMyUnitsConsoleCommand : IDeveloperConsoleCommand
{
    private static readonly string[] s_aliases =
    {
        "grid.kill_allies",
        "grid.kill_self"
    };

    public string Key => "grid.kill_my_units";
    public string Description => "Мгновенно убивает всех юнитов локальной команды на боевой сетке.";
    public string Usage => "grid.kill_my_units";
    public IReadOnlyList<string> Aliases => s_aliases;

    public void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output)
    {
        if (context.GameModel == null || context.TurnService == null)
        {
            output.AppendError("Команда доступна только на боевой сцене с активной сеткой.");
            return;
        }

        var localTeam = context.TurnService.LocalTeam;
        var units = context.GameModel.GetUnits();
        var killed = 0;

        foreach (var unit in units)
        {
            if (unit == null)
                continue;

            if (unit.Team.Value != localTeam)
                continue;

            var damageContext = new DamageContext(int.MaxValue, DamageType.physical, unit);
            damageContext.Target = unit;
            unit.RecieveDamage(damageContext);
            killed++;
        }

        output.AppendLine($"Убито союзных юнитов: {killed}.");
    }
}

