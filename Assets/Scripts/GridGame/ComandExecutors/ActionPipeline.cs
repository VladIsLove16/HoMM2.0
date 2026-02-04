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
        if (!_resolver.TryResolvePlan(ctx, out var plan) || plan.ActionType != type)
        {
            Debug.LogWarning($"[Pipeline] No valid action plan for {type} and ctx {ctx}");
            return false;
        }

        var handler = _resolver.Resolve(plan.ActionType, plan.Context);
        Debug.Log("ActionPipeline  " + type);
        if (handler == null)
        {
            Debug.LogWarning($"[Pipeline] No handler for {type}");
            return false;
        }

        handler.Execute(plan.Context);
        _turnSystem.EndTurn();
        return true;
    }

    public void StartBattle()
    {
        _turnSystem.RunBattle();
    }
}
