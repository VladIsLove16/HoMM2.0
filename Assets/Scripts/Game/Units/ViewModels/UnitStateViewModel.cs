using UniRx;
using UnityEngine;
// Базовый класс состояния
public abstract class UnitStateViewModel
{
    protected UnitViewModel UnitVM;
    protected BattleFieldViewModel BattleVM;

    public abstract UnitActionType ActionType { get; }
    public ReactiveProperty<bool> IsActive { get; } = new();
    public abstract void HandleGridClick(Vector2Int gridPosition);
}
