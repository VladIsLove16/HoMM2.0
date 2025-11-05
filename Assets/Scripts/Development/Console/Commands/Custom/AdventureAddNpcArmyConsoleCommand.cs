using System.Collections.Generic;

public sealed class AdventureAddNpcArmyConsoleCommand : IDeveloperConsoleCommand
{
    private static readonly string[] s_aliases =
    {
        "adv.add_npc_army"
    };

    public string Key => "adventure.add_npc_army";
    public string Description => "Добавляет текущую армию игрока ближайшему NPC.";
    public string Usage => "adventure.add_npc_army";
    public IReadOnlyList<string> Aliases => s_aliases;

    public void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output)
    {
        if (context.AdventureCommander == null)
        {
            output.AppendError("Команда доступна только в Adventure-сцене.");
            return;
        }

        context.AdventureCommander.AddArmyToClosestNpc(output);
    }
}
