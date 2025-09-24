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


    private MoveThenAttackHandler MoveThenAttackHandler;
    private MoveActionHandler MoveActionHandler;
    private RangedAttackHandler RangedAttackHandler;
    private AttackActionHandler AttackActionHandler;
    private SpellActionHandler SpellActionHandler;

    private Dictionary<ActionType, IActionHandler> commandDict = new();
    public ActionResolver(GameModel gameModel, MovementSystem movementSystem)
    {
        MoveThenAttackHandler = new(movementSystem, gameModel);
        MoveActionHandler = new(movementSystem, gameModel);
        RangedAttackHandler = new(movementSystem, gameModel);
        AttackActionHandler = new(movementSystem, gameModel);
        SpellActionHandler = new(movementSystem, gameModel);
        commandDict.Add(ActionType.MoveThenAttack, MoveThenAttackHandler);
        commandDict.Add(ActionType.Attack, AttackActionHandler);
        commandDict.Add(ActionType.Move, MoveActionHandler);
        commandDict.Add(ActionType.RangedAttack, RangedAttackHandler);
        commandDict.Add(ActionType.Spell, SpellActionHandler);
    }
    public IActionHandler Resolve(ActionType type, ActionContext actionContext)
    {
        return commandDict[type];
    }
    public bool Resolve(ActionContext ctx, out IActionHandler handler)
    {
        var resolvedHandlers = new List<IActionHandler>();
        DetermineResolvedHandlers(ctx, resolvedHandlers);
        LogAssertions(resolvedHandlers);
        if (resolvedHandlers.Count > 0)
        {
            handler = resolvedHandlers[0];
            ActionResolved?.Invoke((handler,ctx));
            return true;
        }
        else
            handler = null;
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
        Debug.Log("resolved handlers: " + stringBuilder.ToString());
        if (resolvedHandlers.Count > 1)
            Debug.LogAssertion("resolved handlers count > 1");
    }

}

