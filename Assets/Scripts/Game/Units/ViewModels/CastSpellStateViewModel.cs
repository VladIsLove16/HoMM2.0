using UnityEngine;
// Состояние выбора спелла
public class CastSpellStateViewModel : UnitStateViewModel
{
    private SpellData _selectedSpell;

    public override UnitActionType ActionType => UnitActionType.CastSpell;

    public override void HandleGridClick(Vector2Int gridPosition)
    {
        if (SpellSystem.IsValidTarget(_selectedSpell, gridPosition))
        {
            BattleVM.SpellCastCommand.Execute(
                new SpellCastContext(_selectedSpell, UnitVM, gridPosition));
            BattleVM.ChangeState(new IdleStateViewModel());
        }
    }
}
