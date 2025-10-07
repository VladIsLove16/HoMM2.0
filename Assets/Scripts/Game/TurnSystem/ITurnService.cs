using System;
using UniRx;

public interface ITurnStateViewModel
{
    public IReadOnlyReactiveProperty<ICombatObject> ActiveObject { get; }
    public IReadOnlyReactiveProperty<BattleState> BattleState { get; }
    bool IsMyTurn { get; }
    Team LocalTeam { get; }
}

public interface ITurnCommands
{
    void ConfigureLocalSide(Team team);
    void StartGridPlacementPhase();
    void RunBattle();
    void EndTurn();
    void AddCombatUnit(ICombatObject unit);
    void ClearUnits();
}

public interface ITurnService : ITurnStateViewModel, ITurnCommands
{
}

