using System.Collections.Generic;

public sealed class GridKillEnemyUnitsConsoleCommand : IDeveloperConsoleCommand
{
    private static readonly string[] s_aliases =
    {
        "grid.kill_enemies",
        "grid.kill_opponent"
    };

    public string Key => "grid.kill_enemy_units";
    public string Description => "Мгновенно убивает всех юнитов противника на боевой сетке.";
    public string Usage => "grid.kill_enemy_units";
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

            if (unit.Team.Value == localTeam)
                continue;

            // Наносим заведомо смертельный урон через доменную модель,
            // чтобы корректно отработали события смерти и TurnService.
            var damageContext = new DamageContext(int.MaxValue, DamageType.physical, unit);
            damageContext.Target = unit;
            unit.RecieveDamage(damageContext);
            killed++;
        }

        output.AppendLine($"Убито вражеских юнитов: {killed}.");
    }
}

