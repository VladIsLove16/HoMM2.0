using System;
using System.Collections.Generic;
using UniRx;

public class UnitTurnPanelViewModel
{
    private readonly TurnSystem _turnSystem;
    public List<UnitTurnInfo> turnList = new();
    public ReactiveProperty<int> TurnNumber = new(0);
    public ReactiveProperty<ICombatObject> ActiveUnit = new();
    public Action<UnitTurnInfo> CombatUnitsAdded;
    public UnitTurnPanelViewModel(TurnSystem turnSystem)
    {
        _turnSystem = turnSystem;
        _turnSystem.CombatUnitsAdded += OnCombatUnitsAdded;
        _turnSystem.ActiveObject.Subscribe(ActiveObjectChanged);
        _turnSystem.TurnNumber.Subscribe(TurnNumberChanged);
    }

    private void ActiveObjectChanged(ICombatObject combatObject)
    {
        ActiveUnit.SetValueAndForceNotify(combatObject);
    }

    private void TurnNumberChanged(int turn)
    {
        TurnNumber.SetValueAndForceNotify(turn + 1);
    }

    private void OnCombatUnitsAdded(UnitTurnInfo unitTurnInfo)
    {
        UnitTurnInfo viewInfo = new UnitTurnInfo(unitTurnInfo.Unit, unitTurnInfo.Turn + 1);
        turnList.Add(viewInfo);
        CombatUnitsAdded?.Invoke(viewInfo);
    }
}
