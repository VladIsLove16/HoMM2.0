using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
public class TurnSystem
{
    public List<ICombatObject> CombatUnits { get; } = new List<ICombatObject>();
    public ReactiveProperty<int> TurnNumber = new(0);
    public ReactiveProperty<ICombatObject> ActiveObject = new();
    public ReactiveProperty<BattleState> CurrentBattleState = new();
    public Action<UnitTurnInfo> CombatUnitsAdded;
    protected Dictionary<int, List<ICombatObject>> turnDict = new();
    int turnTowards = 3;
    public Team MyTeam { get; private set; } = Team.Blue;
    public bool IsMyTurn
    {
        get
        {
            var active = ActiveObject != null ? ActiveObject.Value : null;
            if (active == null) return false;
            return active.Team == MyTeam;
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
        MyTeam = team;
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
        // Если удалили активного — сразу перейти к следующему
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
        // Найти первого живого юнита в актуальном слайсе; если пусто — перелистнуть
        if (!EnsureCurrentTurnListIsValid())
        {
            // если после очистки нет доступных юнитов — бой может быть закончен
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
        // Удаляем текущего активного из очереди
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
        // Если удалили все текущие — перейти на следующий срез
        if (!turnDict.ContainsKey(TurnNumber.Value))
        {
            // сдвиг вперёд до ближайшего непустого ключа
            while (!turnDict.ContainsKey(TurnNumber.Value) && turnDict.Count > 0)
            {
                // подобрать минимальный существующий ключ >= текущего
                int nextKey = turnDict.Keys.OrderBy(x => x).FirstOrDefault(x => x >= TurnNumber.Value);
                if (nextKey == 0 && !turnDict.ContainsKey(nextKey)) break;
                TurnNumber.Value = nextKey;
            }
        }
    }

    private bool EnsureCurrentTurnListIsValid()
    {
        // найти ближайший ключ с непустым листом
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
            // удалить из текущего среза всех, кого нет в CombatUnits (мертвые/удалённые)
            list.RemoveAll(u => u == null || !CombatUnits.Contains(u));
            if (list.Count == 0)
            {
                OnNoTurnUnitsLeft();
                // и продолжаем цикл, чтобы найти следующий валидный
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
