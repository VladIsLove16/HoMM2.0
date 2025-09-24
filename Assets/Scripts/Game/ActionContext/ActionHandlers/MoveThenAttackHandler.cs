using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class MoveThenAttackHandler : IActionHandler
{
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
        //_moveActionHandler.Execute(ctx);
        //ActionContext afterMover = new();
        _attackActionHandler.Execute(ctx);
    }
    public bool CanExecute(ActionContext ctx)
    {
        if (!_moveActionHandler.CanExecute(ctx))
            return false;
        if (!_attackActionHandler.CanExecute(ctx))
            return false;
        return true;
    }
}
    //public bool CanShowPreview(ActionContext ctx)
    //{
    //    return ctx.TargetObject != null;
    //}
   
    //public ActionPreview GetPreview(ActionContext ctx)
    //{
    //    int moveSpeed = _activeUnit.Stats.MoveSpeed;
    //    var route = GetRoute(ctx);
    //    var moveRoute = GetMoveRoute(ctx);
    //    var inaccessRoute = GetInaccessibleRoute(route, moveRoute);
    //    var reachableCells = _movementSystem.GetReachableCells(_activeUnit.Position, _activeUnit.Stats.MoveSpeed);

    //    DamageContext damage = null;
    //    if(_activeUnit is IDamageSource source)
    //    {
    //        damage = source.SimulateSendDamage(new(ctx.TargetObject));
    //    }

    //    return new ActionPreview
    //    {
    //        MoveRoute = moveRoute,
    //        InaccessibleRoute = inaccessRoute,
    //        ReachableCells = reachableCells,
    //        IsActionAvailable = CanHandle(ctx),
    //        Damage = damage
    //    };

