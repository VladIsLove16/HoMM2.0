using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class MoveThenAttackHandler : IActionHandler
{
    public ActionType ActionType
    {
        get
        {
            return ActionType.MoveThenAttack;
        }
    }
    private MovementSystem _movementSystem;
    private GameModel _gameModel;
    private MoveActionHandler _moveActionHandler;
    private AttackActionHandler _attackActionHandler;
    public MoveThenAttackHandler(MovementSystem movementSystem, GameModel gameModel)
    {
        _movementSystem = movementSystem;
        _gameModel = gameModel;
        _moveActionHandler = new(_movementSystem, gameModel);
        _attackActionHandler = new(_movementSystem, gameModel);
    }
    public void Execute(ActionContext ctx)
    {
        GetContexts(ctx,out var moveActionContext,out var attackActionContext);
        _moveActionHandler.Execute(moveActionContext);
        _attackActionHandler.Execute(attackActionContext);
    }
    public bool CanExecute(ActionContext ctx)
    {
        GetContexts(ctx, out var moveActionContext, out var attackActionContext);
        if (!_moveActionHandler.CanExecute(moveActionContext))
            return false;
        if (!_attackActionHandler.CanExecute(attackActionContext))
            return false;
        return true;
    }

    public PreviewResult GetPreview(ActionContext actionContext)
    {
        PreviewResult previewResult = new();

        GetPreviews(actionContext, out var movePreview, out var attackPreview);
        previewResult.Add(movePreview.ToDictionary());
        previewResult.Add(attackPreview.ToDictionary());
        return previewResult;
    }

    private void GetPreviews(ActionContext actionContext, out PreviewResult movePreview, out PreviewResult attackPreview)
    {
        ActionContext moveActionContext, attackActionContext;
        GetContexts(actionContext, out moveActionContext, out attackActionContext);

        movePreview = _moveActionHandler.GetPreview(moveActionContext);
        attackPreview = _attackActionHandler.GetPreview(attackActionContext);
    }

    private static void GetContexts(ActionContext actionContext, out ActionContext moveActionContext, out ActionContext attackActionContext)
    {
        moveActionContext = new(actionContext);
        moveActionContext.TargetCell = actionContext.AttackFromCell;
        attackActionContext = new(actionContext);
        attackActionContext.FromCell = actionContext.AttackFromCell;
    }
}
