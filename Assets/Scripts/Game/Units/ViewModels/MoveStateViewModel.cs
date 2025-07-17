using System.Collections.Generic;
using UnityEngine;
// Состояние движения
public class MoveStateViewModel : UnitStateViewModel
{
    public override UnitActionType ActionType => UnitActionType.Move;

    private List<Vector2Int> _reachableCells = new();

    public override void HandleGridClick(Vector2Int gridPosition)
    {
        if (_reachableCells.Contains(gridPosition))
        {
            UnitVM.MoveCommand.Execute(gridPosition);
            BattleVM.ChangeState(new IdleStateViewModel());
        }
    }
}
