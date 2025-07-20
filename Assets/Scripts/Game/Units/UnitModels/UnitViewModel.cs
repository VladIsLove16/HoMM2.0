using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;
public class CommandResult
{
    public bool Success { get; set; }
}
public class MoveCommandResult : CommandResult
{
    public Vector2Int Position;
}
public class AttackCommandResult : CommandResult
{
    public IDamagable Target;
}
public class AttackFromPosInfoCommandResult : CommandResult
{
    public IDamagable Target;
    public Vector2Int Position;
}
public class UnitViewModel : IDisposable
{
    public UnitModel Model { get; }

    public ReactiveCommand<MoveCommandResult> MoveCommand { get; }
    public ReactiveCommand<AttackCommandResult> AttackCommand { get; }
    public ReactiveCommand<AttackFromPosInfoCommandResult> AttackFromPosCommand { get; }

    public readonly Subject<Unit> OnHit = new();
    public readonly Subject<IDamagable> OnStartAttacking = new();
    public readonly Subject<Unit> OnAttacked = new();
    public readonly Subject<Unit> OnDeath = new();
    public readonly Subject<Unit> OnTurnStarted = new();
    public readonly Subject<Vector2Int> OnStartMovingTo = new();
    public readonly Subject<Vector2Int> OnEndMovingTo = new();
    private CompositeDisposable _disposables = new();
    private MovementSystem _movementSystem;

    public UnitViewModel(UnitModel model, MovementSystem movementSystem)
    {
        Model = model;
        _movementSystem = movementSystem;

        MoveCommand = new ReactiveCommand<MoveCommandResult>();
        AttackCommand = new ReactiveCommand<AttackCommandResult>();
        AttackFromPosCommand = new ReactiveCommand<AttackFromPosInfoCommandResult>();

        MoveCommand.Subscribe(MoveTo).AddTo(_disposables);
        AttackCommand.Subscribe(Attack).AddTo(_disposables);
        AttackFromPosCommand.Subscribe(Attack).AddTo(_disposables);

        // Прокидываем события из модели
        Model.Hitted += _ => OnHit.OnNext(Unit.Default);
        Model.Attacked += () => OnAttacked.OnNext(Unit.Default);
        Model.Died += () => OnDeath.OnNext(Unit.Default);
        Model.TurnStarted += () => OnTurnStarted.OnNext(Unit.Default);
    }

    public IEnumerator MoveAlongRoute(List<Vector2Int> route)
    {
        OnStartMovingTo.OnNext(route[route.Count - 1]);
        foreach (var position in route)
        {
            //if (!CanMove(position))
            //    yield break;

            MoveCommand.Execute(new MoveCommandResult { Success = true, Position = position });

            yield return new WaitUntil(() => Model.Position.Value == position);

            yield return new WaitForSeconds(0.1f); // Optional delay between steps
        }
        OnEndMovingTo.OnNext(route[route.Count - 1]);
        SetModelActable(true);
    }

    public IEnumerator Attack(IDamagable target)
    {
        OnStartAttacking.OnNext(target);
        AttackCommand.Execute(new AttackCommandResult { Success = true, Target = target });
        yield return new WaitForSeconds(0.1f); // Optional delay between steps
        SetModelActable(true);
    }
    private void MoveTo(MoveCommandResult result)
    {
        Model.Position.Value = result.Position;
        SetModelActable (false);
    }

    private void Attack(AttackCommandResult result)
    {
        var fromInfo = new AttackFromPosInfoCommandResult
        {
            Position = Model.Position.Value,
            Target = result.Target
        };
        Attack(fromInfo);
    }

    private void Attack(AttackFromPosInfoCommandResult info)
    {
        if (CanAttack(info.Target, info.Position))
        {
            var dctx = new DamageContext(Model.ModifiedStats.Damage, DamageType.physical, Model);
            info.Target.ReceiveDamage(dctx);
        }
    }

    private bool CanAttack(IDamagable target, Vector2Int fromPosition)
    {
        if (!Model.CanAct.Value) return false;
        return IsInAttackRange(target.Position, fromPosition);
    }

    private bool IsInAttackRange(Vector2Int targetPos, Vector2Int fromPos)
    {
        return Vector2Int.Distance(fromPos, targetPos) <= Model.ModifiedStats.AttackRange;
    }

    private void SetModelActable(bool state)
    {
        Model.CanAct.Value = state;
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }

    internal List<IActionHandler> GetAvailableAction()
    {
        var list = new List<IActionHandler>();  
        if(Model.CanMove.Value)
        {
            list.Add(new MoveActionHandler());
        }
        if(Model.CanAct.Value)
        {
            list.Add(new MoveThenAttackHandler());
        }
        if(Model.ModifiedStats.AttackRange>1)
        {
            list.Add(new RangedAttackHandler());
        }
        return list;
    }
}
