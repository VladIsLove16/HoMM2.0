using System.Collections.Generic;

public sealed class AdventureCreateNpcConsoleCommand : IDeveloperConsoleCommand
{
    private static readonly string[] s_aliases =
    {
        "adventure.spawn_npc",
        "adv.create_npc"
    };

    public string Key => "adventure.create_npc";
    public string Description => "Создает NPC рядом с игроком на adventure-сцене.";
    public string Usage => "adventure.create_npc";
    public IReadOnlyList<string> Aliases => s_aliases;

    public void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output)
    {
        if (context.AdventureCommander == null)
        {
            output.AppendError("Команда доступна только в Adventure-сцене.");
            return;
        }

        context.AdventureCommander.CreateNpc(output);
    }
}
