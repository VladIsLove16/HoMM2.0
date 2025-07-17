using System;
using UniRx;
using UnityEngine;

public class CommandResult
{
    public bool Success { get; set; }
    public int RemainingMovementPoints { get; set; }
    public int DamageDealt { get; set; }
}
public class MoveCommandInput : CommandResult
{
    public Vector2Int Position { get; set; }
}
public class AttackCommandInput : CommandResult
{
    public UnitViewModel Target { get; set; }
}
public class AttackFromPosCommandInput : CommandResult
{
    public UnitViewModel Target { get; set; }
    public Vector2Int Position { get; set; }
}
public class UnitViewModel : IDisposable
{
    public UnitModel Model { get; }
    public ReactiveCommand<MoveCommandInput> MoveCommand { get; }
    public ReactiveCommand<AttackCommandInput> AttackCommand { get; }
    public ReactiveCommand<AttackFromPosCommandInput> AttackFromPosCommand { get; }
    private MovementSystem _movementSystem;

    private CompositeDisposable _disposables = new();

    public UnitViewModel(UnitModel model, MovementSystem movementSystem)
    {
        Model = model;
        _movementSystem = movementSystem;

        MoveCommand = new ReactiveCommand<MoveCommandInput>();
        AttackCommand = new ReactiveCommand<AttackCommandInput>();
        AttackFromPosCommand = new ReactiveCommand<AttackFromPosCommandInput>();

        MoveCommand.Subscribe(MoveTo).AddTo(_disposables);
        AttackCommand.Subscribe(Attack).AddTo(_disposables);
        AttackFromPosCommand.Subscribe(Attack).AddTo(_disposables);
    }

    private CommandResult MoveTo(MoveCommandInput moveCommandInput)
    {
        Vector2Int position = moveCommandInput.Position;
        if (CanMove(position))
        {
            int movementCost = Vector2Int.Distance(Model.Position.Value, position);
            Model.Position.Value = position;
            Model.CanAct.Value = false;

            return new CommandResult
            {
                Success = true,
                Message = "Moved successfully.",
                RemainingMovementPoints = Model.MovementRange.Value - movementCost
            };
        }
        return new CommandResult
        {
            Success = false,
            Message = "Cannot move to the specified position."
        };
    }

    private CommandResult Attack(UnitViewModel targetUnit)
    {
        if (CanAttack(targetUnit))
        {
            int damageDealt = Model.AttackPower.Value;
            targetUnit.Model.Health.Value -= damageDealt;
            Model.CanAct.Value = false;

            return new CommandResult
            {
                Success = true,
                Message = "Attack successful.",
                DamageDealt = damageDealt
            };
        }
        return new CommandResult
        {
            Success = false,
            Message = "Cannot attack the target."
        };
    }

    private CommandResult Attack(AttackFromPosInfo attackFromPosInfo)
    {
        if (CanAttack(attackFromPosInfo.target, attackFromPosInfo.pos))
        {
            return Attack(attackFromPosInfo.target);
        }
        return new CommandResult
        {
            Success = false,
            Message = "Cannot attack from the specified position."
        };
    }

    private bool CanMove(Vector2Int targetPos)
    {
        return Model.CanAct.Value &&
               _movementSystem.GetReachableCells(Model.Position.Value, Model.MovementRange.Value).Contains(targetPos);
    }

    private bool CanAttack(UnitViewModel targetUnit)
    {
        return Model.CanAct.Value && IsInAttackRange(targetUnit.Model.Position.Value);
    }

    private bool CanAttack(UnitViewModel targetUnit, Vector2Int fromPosition)
    {
        return Model.CanAct.Value && IsInAttackRange(targetUnit.Model.Position.Value, fromPosition);
    }

    private bool IsInAttackRange(Vector2Int targetPos)
    {
        return Vector2Int.Distance(Model.Position.Value, targetPos) <= Model.AttackRange.Value;
    }

    private bool IsInAttackRange(Vector2Int targetPos, Vector2Int fromPosition)
    {
        return Vector2Int.Distance(fromPosition, targetPos) <= Model.AttackRange.Value;
    }

    public void Dispose() => _disposables.Dispose();
}
