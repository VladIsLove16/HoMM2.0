using System;
using UniRx;

public class TurnService : ITurnService
{
    private readonly TurnSystem _turnSystem;

    public TurnService(TurnSystem turnSystem)
    {
        _turnSystem = turnSystem ?? throw new ArgumentNullException(nameof(turnSystem));
    }

    public IReadOnlyReactiveProperty<BattleState> BattleState => _turnSystem.CurrentBattleState;
    public IReadOnlyReactiveProperty<ICombatObject> ActiveObject => _turnSystem.ActiveObject;
    public bool IsMyTurn => _turnSystem.IsMyTurn;
    public Team LocalTeam => _turnSystem.LocalTeam;

    public void ConfigureLocalSide(Team team)
    {
        _turnSystem.ConfigureLocalSide(team);
    }

    public void StartGridPlacementPhase()
    {
        _turnSystem.StartGridPlacementPhase();
    }

    public void RunBattle()
    {
        _turnSystem.RunBattle();
    }

    public void EndTurn()
    {
        _turnSystem.EndTurn();
    }

    public void AddCombatUnit(ICombatObject unit)
    {
        _turnSystem.AddCombatUnit(unit);
    }

    public void ClearUnits()
    {
        _turnSystem.ClearUnits();
    }
}
