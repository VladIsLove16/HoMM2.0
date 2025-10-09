using System;
using System.Collections.Generic;
using UniRx;

public interface ITurnService
{
    GameMode Mode { get; }
    Team LocalTeam { get; }
    ICombatObject ActiveObject { get; }
    BattleState BattleState { get; }
    int TurnNumber { get; }
    IReadOnlyList<ICombatObject> CombatUnits { get; }
    bool IsMyTurn { get; }

    IObservable<ICombatObject> ActiveObjectStream { get; }
    IObservable<BattleState> BattleStateStream { get; }
    IObservable<int> TurnNumberStream { get; }
    IObservable<UnitTurnInfo> UnitAddedStream { get; }

    void ConfigureControl(Team team, GameMode mode);
    void StartGridPlacementPhase();
    void RunBattle();
    void EndTurn();
    void AddCombatUnit(ICombatObject unit);
    void RemoveCombatUnit(ICombatObject unit);
    void ClearUnits();
}

public interface ITurnStateViewModel
{
    IReadOnlyReactiveProperty<ICombatObject> ActiveObject { get; }
    IReadOnlyReactiveProperty<BattleState> BattleStateProperty { get; }
    IReadOnlyReactiveProperty<int> TurnNumber { get; }
    IObservable<UnitTurnInfo> UnitAddedStream { get; }
    bool IsMyTurn { get; }
    Team LocalTeam { get; }
    IReadOnlyList<ICombatObject> CombatUnits { get; }
}

public interface ITurnQueue
{
    ICombatObject PeekNext();
    void Enqueue(int turn, ICombatObject unit);
    ICombatObject DequeueCurrent(ICombatObject current);
    bool EnsureValid(int currentTurn, IList<ICombatObject> activeUnits);
    void AdvanceTurn();
    void Clear();
}
