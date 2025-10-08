using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;
public class TurnSystem
{
    public List<ICombatObject> CombatUnits { get; } = new List<ICombatObject>();
    public ReactiveProperty<int> TurnNumber = new(0);
    public ReactiveProperty<ICombatObject> ActiveObject = new();
    public ReactiveProperty<BattleState> CurrentBattleState = new();
    public Action<UnitTurnInfo> CombatUnitsAdded;
    protected Dictionary<int, List<ICombatObject>> turnDict = new();
    int turnTowards = 3;
    public Team LocalTeam { get; private set; } = Team.Blue;
    private IGameModeProvider _gameModeProvider;

    [Inject]
    public void Construct([InjectOptional] IGameModeProvider gameModeProvider = null)
    {
        _gameModeProvider = gameModeProvider;
    }

    private GameMode CurrentGameMode => (_gameModeProvider ?? GameConfigurationService.Instance).CurrentGameMode;

    public bool IsMyTurn
    {
        get
        {
            if (CurrentGameMode == GameMode.SinglePlayer)
                return true;
            var active = ActiveObject != null ? ActiveObject.Value : null;
            if (active == null) return false;
            return active.Team == LocalTeam;
        }
    }
    private bool IsCombatEnded(out Team winningTeam)
    {
        var aliveTeams = CombatUnits.Select(u => u.Team).Distinct().ToList();
        if (aliveTeams.Count <= 1 && aliveTeams.Count > 0)
        {
            winningTeam = aliveTeams[0];
            return true;
        }
        winningTeam = Team.Blue;
        return false;
    }
    public void ConfigureLocalSide(Team team)
    {
        LocalTeam = team;
    }
    public virtual void AddCombatUnit(ICombatObject unit)
    {
        if (unit == null) return;
        if (CombatUnits.Contains(unit)) return;
        CombatUnits.Add(unit);
        unit.Died += RemoveCombatUnit;
    }
    public void RemoveCombatUnit(ICombatObject unit)
    {
        if (unit == null) return;
        CombatUnits.Remove(unit);
        RemoveFromTurnDict(unit);
        // ���� ������� ��������� � ����� ������� � ����������
        if (ActiveObject.Value == unit)
        {
            EndTurn();
        }
        if(IsCombatEnded(out var winningTeam))
        {
            var newBattleState = winningTeam == Team.Blue ? BattleState.blueTeamWins : BattleState.redTeamWins;
            CurrentBattleState.SetValueAndForceNotify(newBattleState);
        }
    }
    public void ClearUnits()
    {
        CombatUnits.Clear();
        turnDict.Clear();
        TurnNumber.Value = 0;
        ActiveObject.Value = null;
    }
    public void StartGridPlacementPhase()
    {
        CurrentBattleState.SetValueAndForceNotify(BattleState.replacement);
    }
    public void RunBattle()
    {
        turnDict.Clear();
        TurnNumber.Value = 0;
        ActiveObject.Value = null;
        AddFirstUnits();
        TakeTurn();
        CurrentBattleState.SetValueAndForceNotify(BattleState.inProgress);
    }
    protected virtual ICombatObject GetFirstObject()
    {
        EnsureCurrentTurnListIsValid();
        return turnDict[TurnNumber.Value][0];
    }
    public void TakeTurn()
    {
        if (!EnsureCurrentTurnListIsValid())
        {
            return;
        }
        ICombatObject combatObject = GetFirstObject();
        if (ActiveObject.Value == combatObject)
        {
            Debug.LogWarning("Turn already taken by first unit " + ActiveObject.Value.ToString());
            return;
        }
        combatObject.TakeTurn();
        ActiveObject.SetValueAndForceNotify(combatObject);
    }
    public void EndTurn()
    {
        if (ActiveObject.Value != null)
        {
            ActiveObject.Value.EndTurn();
        }
        DequeueUnit();
        TakeTurn();
    }

    private void AddFirstUnits()
    {
        if(CombatUnits == null || CombatUnits.Count == 0)
        {
            Debug.LogWarning("CombatUnits not added");
            return;
        }
        AddUnitsUntil(turnTowards);
    }

    private void AddUnitsUntil(int turn)
    {
        for (int i = turnDict.Count; i < turn; i++)
        {
            if(!turnDict.ContainsKey(i))
                foreach (var unit in CombatUnits)
                {
                    AddUnitToTurn(i, unit);
                }
        }
    }

    private void AddUnitToTurn(int turn, ICombatObject unit)
    {
        if (!turnDict.ContainsKey(turn))
        {
            turnDict[turn] = new();
        }
        if (unit != null)
            turnDict[turn].Add(unit);
        CombatUnitsAdded?.Invoke(new UnitTurnInfo(unit, turn));
    }

    protected virtual ICombatObject DequeueUnit()
    {
        if (!turnDict.ContainsKey(TurnNumber.Value))
        {
            OnNoTurnUnitsLeft();
        }
        EnsureCurrentTurnListIsValid();
        if (!turnDict.ContainsKey(TurnNumber.Value))
        {
            return null;
        }
        if (turnDict[TurnNumber.Value].Count == 0)
        {
            OnNoTurnUnitsLeft();
        }
        if (!turnDict.ContainsKey(TurnNumber.Value) || turnDict[TurnNumber.Value].Count == 0)
        {
            return null;
        }
        // ������� �������� ��������� �� �������
        turnDict[TurnNumber.Value].Remove(ActiveObject.Value);
        if (turnDict[TurnNumber.Value].Count == 0)
        {
            OnNoTurnUnitsLeft();
        }
        EnsureCurrentTurnListIsValid();
        if (!turnDict.ContainsKey(TurnNumber.Value) || turnDict[TurnNumber.Value].Count == 0)
        {
            return null;
        }
        ICombatObject combatUnit = turnDict[TurnNumber.Value][0];
        return combatUnit;
    }

    private void OnNoTurnUnitsLeft()
    {
        if (turnDict.ContainsKey(TurnNumber.Value))
        {
            turnDict.Remove(TurnNumber.Value);
        }
        TurnNumber.SetValueAndForceNotify(TurnNumber.Value + 1);
        if (turnDict.Count <= TurnNumber.Value + turnTowards)
        {
            AddUnitsUntil(TurnNumber.Value + turnTowards);
        }
    }

    private void RemoveFromTurnDict(ICombatObject unit)
    {
        if (unit == null) return;
        foreach (var kv in turnDict.ToList())
        {
            var list = kv.Value;
            if (list == null) continue;
            list.RemoveAll(u => u == null || u.Equals(unit));
            if (list.Count == 0)
            {
                turnDict.Remove(kv.Key);
            }
        }
        if (!turnDict.ContainsKey(TurnNumber.Value))
        {
            while (!turnDict.ContainsKey(TurnNumber.Value) && turnDict.Count > 0)
            {
                int nextKey = turnDict.Keys.OrderBy(x => x).FirstOrDefault(x => x >= TurnNumber.Value);
                if (nextKey == 0 && !turnDict.ContainsKey(nextKey)) break;
                TurnNumber.Value = nextKey;
            }
        }
    }

    private bool EnsureCurrentTurnListIsValid()
    {
        while (true)
        {
            if (!turnDict.ContainsKey(TurnNumber.Value))
            {
                if (turnDict.Count == 0) return false;
                int nextKey = turnDict.Keys.OrderBy(x => x).First();
                TurnNumber.Value = nextKey;
            }
            if (!turnDict.ContainsKey(TurnNumber.Value)) return false;
            var list = turnDict[TurnNumber.Value];
            list.RemoveAll(u => u == null || !CombatUnits.Contains(u));
            if (list.Count == 0)
            {
                OnNoTurnUnitsLeft();
                continue;
            }
            return true;
        }
    }
}

public class TurnSystemDebugger : TurnSystem
{
    protected override ICombatObject GetFirstObject()
    {
        if(turnDict.Count == 0)
        {
            Debug.Log("turnDict.Count == 0");
        }
        if (turnDict.ContainsKey(0) && turnDict[0].Count == 0)
        {
            Debug.Log("turnDict[0].Count == 0");
        }
        return  base.GetFirstObject();
    }
    protected override ICombatObject DequeueUnit()
    {
        ICombatObject @object =  base.DequeueUnit();
        string a = string.Empty;
        foreach (var b in turnDict.ContainsKey(TurnNumber.Value) ? turnDict[TurnNumber.Value] : new List<ICombatObject>())
        {
            a += b.ToString();
        }
        Debug.Log("unit dequied,left units" + a);
        return @object;
    }
    public override void AddCombatUnit(ICombatObject unit)
    {
        Debug.Log("unit added called " + unit.ToString());
        base.AddCombatUnit(unit);
    }
}
