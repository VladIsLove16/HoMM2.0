using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniRx;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Zenject;
public class ActionResolver
{
    [Inject] protected MoveActionHandlerFactory _moveFactory = new();
    [Inject] protected RangedAttackHandlerFactory _rangedFactory = new();
    [Inject] protected MoveThenAttackHandlerFactory _moveThenAttackFactory = new();
    [Inject] protected SpellHandlerFactory _spellHandlerFactory = new();

    public List<IActionHandler> _handlers = new();
    public ReactiveProperty<IActionHandler> CurrentAction = new();
    public ActionResolver()
    {
    }

    /// <summary>
    /// GetActionHandler for ctx
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="handler"></param>
    /// <returns></returns>
    public bool Resolve(ActionContext ctx, out IActionHandler handler)
    {
        var resolvedHandlers = new List<IActionHandler>();
        var moveHandler = _moveFactory.Create(ctx);
        var rangedAttackHandler = _rangedFactory.Create(ctx);
        var moveThenAttackHandler = _moveThenAttackFactory.Create(ctx);
        var spellHandlerHandler = _spellHandlerFactory.Create(ctx);
        if (moveHandler.CanExecute(ctx))
            resolvedHandlers.Add(moveHandler);
        if (rangedAttackHandler.CanExecute(ctx))
            resolvedHandlers.Add(rangedAttackHandler);
        if (moveThenAttackHandler.CanExecute(ctx))
            resolvedHandlers.Add(moveThenAttackHandler);
        if (spellHandlerHandler.CanExecute(ctx))
            resolvedHandlers.Add(spellHandlerHandler);
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var resolvedHandler in resolvedHandlers)
        {
            stringBuilder.Append(resolvedHandler.ToString());
        }
        Debug.Log("resolved handlers: " + stringBuilder.ToString());
        if (resolvedHandlers.Count > 1)
            Debug.LogAssertion("resolved handlers count > 1");
        if (resolvedHandlers.Count > 0)
        {
            handler = resolvedHandlers[0];
            return true;
        }
        else
            handler = null;
        return false;
    }
    protected bool CanHandle(IActionHandler handler, ActionContext ctx)
    {
        return handler.CanHandle(ctx);
    }
    /// <summary>
    /// Execute action forcly
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="ctx"></param>
    private void Execute(IActionHandler handler, ActionContext ctx)
    {
        handler.Execute(ctx);
    }

}

