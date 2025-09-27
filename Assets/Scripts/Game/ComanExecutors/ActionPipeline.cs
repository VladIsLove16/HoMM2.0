using UnityEngine;

public class ActionPipeline
{
    private readonly ActionResolver _resolver;
    private readonly TurnSystem _turnSystem;

    public ActionPipeline(ActionResolver resolver, TurnSystem turnSystem)
    {
        _resolver = resolver;
        _turnSystem = turnSystem;
    }

    public void Execute(ActionType type, ActionContext ctx)
    {
        var handler = _resolver.Resolve(type, ctx);
        if (handler == null)
        {
            Debug.LogWarning($"[Pipeline] No handler for {type}");
            return;
        }

        if (!handler.CanExecute(ctx))
        {
            Debug.LogWarning($"[Pipeline] Handler cannot execute {type} for ctx " + ctx.ToString());
            return;
        }

        handler.Execute(ctx);
    }

    public void StartBattle()
    {
        _turnSystem.RunBattle();
    }
}
