using System.Collections.Generic;
using UnityEngine;

public class ExecuteActionConsoleCommand : IDeveloperConsoleCommand
{
    private static readonly string[] _aliases = { "exec", "action" };

    public string Key => "execute";
    public string Description => "Executes a specific action type with explicit coordinates.";
    public string Usage => "execute <actionType> <fromX> <fromY> <targetX> <targetY> [attackFromX attackFromY]";
    public IReadOnlyList<string> Aliases => _aliases;

    public void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output)
    {
        if (context.GridViewModel == null || context.CommandExecutor == null)
        {
            output.AppendError("Required systems are not available.");
            return;
        }

        if (args == null || args.Count < 5)
        {
            output.AppendError($"Usage: {Usage}");
            return;
        }

        if (!DeveloperConsoleParsing.TryParseEnum<ActionType>(args[0], out var actionType))
        {
            output.AppendError($"Unknown action type '{args[0]}'.");
            return;
        }

        if (!DeveloperConsoleParsing.TryParseVector2Int(args, 1, out var from))
        {
            output.AppendError("Invalid source cell coordinates.");
            return;
        }

        if (!DeveloperConsoleParsing.TryParseVector2Int(args, 3, out var target))
        {
            output.AppendError("Invalid target cell coordinates.");
            return;
        }

        Vector2Int attackFrom = from;
        if (args.Count >= 7)
        {
            if (!DeveloperConsoleParsing.TryParseVector2Int(args, 5, out attackFrom))
            {
                output.AppendError("Invalid attack-from coordinates.");
                return;
            }
        }

        var actionContext = new ActionContext(from, target, SpellType.None, attackFrom);

        //if (!context.GridViewModel.CanExecute(actionType, actionContext))
        //{
        //    output.AppendError($"Action {actionType} cannot be executed with the provided context.");
        //    return;
        //}

        context.CommandExecutor.Execute(actionType, actionContext);
        output.AppendLine($"Executed {actionType} from ({from.x},{from.y}) to ({target.x},{target.y}).");
    }
}
