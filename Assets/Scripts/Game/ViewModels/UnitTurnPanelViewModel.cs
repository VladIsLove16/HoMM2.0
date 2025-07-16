using System;
using System.Collections.Generic;

public class UnitTurnPanelViewModel
{
    private readonly TurnSystem _turnSystem;
    public List<UnitTurnInfo> turnList = new();
    public Action<int> TurnNumberChanged;
    public int TurnNumber
    {
        get
        {
            if (_turnSystem != null)
                return _turnSystem.TurnNumber + 1;
            else return 0;
        }
    }
    public Action ActiveUnitChanged;
    public Action<UnitTurnInfo> CombatUnitsAdded;
    public UnitTurnPanelViewModel(TurnSystem combatSystem)
    {
        _turnSystem = combatSystem;
        _turnSystem.CombatUnitsAdded += OnCombatUnitsAdded;
        _turnSystem.ActiveUnitChanged += () => ActiveUnitChanged.Invoke();
        _turnSystem.TurnNumberChanged += (turn) =>
        {
            TurnNumberChanged?.Invoke(turn + 1);
        };
    }

    private void OnCombatUnitsAdded(UnitTurnInfo unitTurnInfo)
    {
        UnitTurnInfo viewInfo = new UnitTurnInfo(unitTurnInfo.Unit, unitTurnInfo.Turn + 1);
        turnList.Add(viewInfo);
        CombatUnitsAdded?.Invoke(viewInfo);
    }
}
