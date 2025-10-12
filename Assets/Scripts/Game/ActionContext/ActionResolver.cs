using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniRx;
using Unity.Plastic.Newtonsoft.Json.Serialization;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Zenject;
public class ActionResolver
{
    public event Action<(IActionHandler,ActionContext)> ActionResolved;
    public System.Action ActionNotResolved;

    private MoveThenAttackHandler MoveThenAttackHandler;
    private MoveActionHandler MoveActionHandler;
    private RangedAttackHandler RangedAttackHandler;
    private AttackActionHandler AttackActionHandler;
    private SpellActionHandler SpellActionHandler;

    private Dictionary<ActionType, IActionHandler> actionDict = new();
    public ActionResolver(GameModel gameModel, MovementSystem movementSystem)
    {
        MoveThenAttackHandler = new(movementSystem, gameModel);
        MoveActionHandler = new(movementSystem, gameModel);
        RangedAttackHandler = new(movementSystem, gameModel);
        AttackActionHandler = new(movementSystem, gameModel);
        SpellActionHandler = new(movementSystem, gameModel);
        actionDict.Add(ActionType.MoveThenAttack, MoveThenAttackHandler);
        actionDict.Add(ActionType.Attack, AttackActionHandler);
        actionDict.Add(ActionType.Move, MoveActionHandler);
        actionDict.Add(ActionType.RangedAttack, RangedAttackHandler);
        actionDict.Add(ActionType.Spell, SpellActionHandler);
    }
    public IActionHandler Resolve(ActionType type, ActionContext actionContext)
    {
        return actionDict[type];
    }
    public bool Resolve(ActionContext ctx, out IActionHandler handler)
    {
        var resolvedHandlers = new List<IActionHandler>();
        DetermineResolvedHandlers(ctx, resolvedHandlers);
        LogAssertions(resolvedHandlers);
        if (resolvedHandlers.Count > 0)
        {
            handler = resolvedHandlers[0];
            Debug.Log("action resolver Invoke");
            ActionResolved?.Invoke((handler,ctx));
            return true;
        }
        else
        {
            ActionNotResolved?.Invoke();
            handler = null;
        }
        return false;
    }

    private void DetermineResolvedHandlers(ActionContext ctx, List<IActionHandler> resolvedHandlers)
    {
        if (MoveThenAttackHandler.CanExecute(ctx))
            resolvedHandlers.Add(MoveThenAttackHandler);
        if (MoveActionHandler.CanExecute(ctx))
            resolvedHandlers.Add(MoveActionHandler);
        if (RangedAttackHandler.CanExecute(ctx))
            resolvedHandlers.Add(RangedAttackHandler);
        if (AttackActionHandler.CanExecute(ctx))
            resolvedHandlers.Add(AttackActionHandler);
        if (SpellActionHandler.CanExecute(ctx))
            resolvedHandlers.Add(SpellActionHandler);
    }

    private void LogAssertions(List<IActionHandler> resolvedHandlers)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var resolvedHandler in resolvedHandlers)
        {
            stringBuilder.Append(resolvedHandler.ToString());
        }
        if (resolvedHandlers.Count > 1)
            Debug.LogWarning("resolved handlers count > 1" );
        Debug.Log("resolved handlers: " + stringBuilder.ToString());
    }

}

