using UnityEngine;

public class ActionPipeline
{
    private readonly ActionResolver _resolver;
    private readonly ITurnService _turnSystem;

    public ActionPipeline(ActionResolver resolver, ITurnService turnSystem)
    {
        _resolver = resolver;
        _turnSystem = turnSystem;
    }

    public bool Execute(ActionType type, ActionContext ctx)
    {
        var handler = _resolver.Resolve(type, ctx);
        Debug.Log("ActionPipeline  " + type);
        if (handler == null)
        {
            Debug.LogWarning($"[Pipeline] No handler for {type}");
            return false;
        }

        if (!handler.CanExecute(ctx))
        {
            Debug.LogWarning($"[Pipeline] Handler cannot execute {type} for ctx " + ctx.ToString());
            return false;
        }

        handler.Execute(ctx);
        _turnSystem.EndTurn();
        return true;
    }

    public void StartBattle()
    {
        _turnSystem.RunBattle();
    }
}
