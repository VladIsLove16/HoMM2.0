using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

public class TurnService : ITurnService
{
    private readonly ITurnQueue _queue;
    private readonly List<ICombatObject> _combatUnits = new();
    private readonly ReactiveProperty<ICombatObject> _activeObject = new();
    private readonly ReactiveProperty<BattleState> _battleState = new(BattleState.none);
    private readonly ReactiveProperty<int> _turnNumber = new(0);
    private readonly Subject<UnitTurnInfo> _unitAddedSubject = new();
    private GameMode _mode;

    public TurnService(
        ITurnQueue queue = null,
        GameMode mode = GameMode.SinglePlayer,
        Team localTeam = Team.Blue)
    {
        _queue = queue ?? new TurnQueue();
        _mode = mode;
        LocalTeam = localTeam;
    }

    public GameMode Mode => _mode;
    public Team LocalTeam { get; private set; }
    public ICombatObject ActiveObject => _activeObject.Value;
    public BattleState BattleState => _battleState.Value;
    public int TurnNumber => _turnNumber.Value;
    public IReadOnlyList<ICombatObject> CombatUnits => _combatUnits;
    public bool IsMyTurn =>
        _mode == GameMode.SinglePlayer ||
        (_activeObject.Value != null && _activeObject.Value.Team == LocalTeam);

    public IObservable<ICombatObject> ActiveObjectStream => _activeObject;
    public IObservable<BattleState> BattleStateStream => _battleState;
    public IObservable<int> TurnNumberStream => _turnNumber;
    public IObservable<UnitTurnInfo> UnitAddedStream => _unitAddedSubject;

    public void ConfigureControl(Team team, GameMode mode)
    {
        LocalTeam = team;
        _mode = mode;
    }

    public void StartGridPlacementPhase()
    {
        _battleState.Value = BattleState.replacement;
    }

    public void RunBattle()
    {
        _queue.Clear();
        _turnNumber.Value = 0;
        _activeObject.Value = null;

        foreach (var unit in _combatUnits)
        {
            QueueUnit(0, unit);
        }

        _battleState.Value = BattleState.inProgress;
        TakeTurn();
    }

    public void EndTurn()
    {
        var current = _activeObject.Value;
        current?.EndTurn();

        if (current != null && _combatUnits.Contains(current))
        {
            QueueUnit(_turnNumber.Value + 1, current);
        }

        var next = _queue.DequeueCurrent(current);
        if (next == null)
        {
            _turnNumber.Value++;
            _queue.AdvanceTurn();
            TakeTurn();
        }
        else
        {
            _activeObject.Value = next;
            next.TakeTurn();
        }
    }

    public void AddCombatUnit(ICombatObject unit)
    {
        if (unit == null || _combatUnits.Contains(unit))
        {
            return;
        }

        _combatUnits.Add(unit);
        unit.Died += OnUnitDied;
    }

    public void RemoveCombatUnit(ICombatObject unit)
    {
        HandleUnitRemoval(unit);
    }

    public void ClearUnits()
    {
        foreach (var unit in _combatUnits)
        {
            unit.Died -= OnUnitDied;
        }

        _combatUnits.Clear();
        _queue.Clear();
        _turnNumber.Value = 0;
        _activeObject.Value = null;
        _battleState.Value = BattleState.none;
    }

    private void QueueUnit(int turn, ICombatObject unit)
    {
        if (unit == null)
        {
            return;
        }

        _queue.Enqueue(turn, unit);
        _unitAddedSubject.OnNext(new UnitTurnInfo(unit, turn));
    }

    private void TakeTurn()
    {
        if (!_queue.EnsureValid(_turnNumber.Value, _combatUnits))
        {
            _activeObject.Value = null;
            return;
        }

        var next = _queue.PeekNext();
        if (next == null)
        {
            _activeObject.Value = null;
            return;
        }

        _activeObject.Value = next;
        next.TakeTurn();
    }

    private void OnUnitDied(ICombatObject unit)
    {
        HandleUnitRemoval(unit);
    }

    private void HandleUnitRemoval(ICombatObject unit)
    {
        if (unit == null)
        {
            return;
        }

        unit.Died -= OnUnitDied;

        if (!_combatUnits.Remove(unit))
        {
            return;
        }

        _queue.EnsureValid(_turnNumber.Value, _combatUnits);

        if (_activeObject.Value == unit)
        {
            _activeObject.Value = null;
            EndTurn();
        }

        if (IsCombatEnded(out var winner))
        {
            _battleState.Value = winner switch
            {
                Team.Blue => BattleState.blueTeamWins,
                Team.Red => BattleState.redTeamWins,
                _ => BattleState.none
            };
        }
    }

    private bool IsCombatEnded(out Team winningTeam)
    {
        var aliveTeams = _combatUnits
            .Select(u => u.Team)
            .Distinct()
            .ToList();

        if (aliveTeams.Count == 1)
        {
            winningTeam = aliveTeams[0];
            return true;
        }

        winningTeam = Team.None;
        return false;
    }
}
