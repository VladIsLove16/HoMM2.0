using System.Collections.Generic;

public class StartBattleConsoleCommand : IDeveloperConsoleCommand
{
    private static readonly string[] _aliases = { "start", "run_battle" };

    public string Key => "start_battle";
    public string Description => "Starts the battle loop using the current turn system.";
    public string Usage => "start_battle";
    public IReadOnlyList<string> Aliases => _aliases;

    public void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output)
    {
        if (context.CommandExecutor == null)
        {
            output.AppendError("Game command executor is not available.");
            return;
        }

        context.CommandExecutor.StartBattle();
        output.AppendLine("Battle started.");
    }
}
