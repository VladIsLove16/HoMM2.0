using System.Collections.Generic;

public class EndTurnConsoleCommand : IDeveloperConsoleCommand
{
    private static readonly string[] _aliases = { "end", "endturn" };

    public string Key => "end_turn";
    public string Description => "Ends the current active unit's turn.";
    public string Usage => "end_turn";
    public IReadOnlyList<string> Aliases => _aliases;

    public void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output)
    {
        if (context.TurnService == null)
        {
            output.AppendError("Turn system is not available.");
            return;
        }

        context.TurnService.EndTurn();
        output.AppendLine("Turn ended.");
    }
}
