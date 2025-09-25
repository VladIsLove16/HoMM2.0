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
    // Локальная сторона: синий == хост. Используется для определения "мой ход"
    public bool MyIsBlueTeam { get; private set; }
    public Material MyTeamMaterial { get; private set; }
    public bool IsMyTurn
    {
        get
        {
            var active = ActiveObject != null ? ActiveObject.Value : null;
            if (active == null) return false;
            return active.IsBlueTeam == MyIsBlueTeam;
        }
    }
    int turnTowards = 3;
    public Action<UnitTurnInfo> CombatUnitsAdded;
    protected Dictionary<int, List<ICombatObject>> turnDict = new();
    public event Action<BattleState> BattleStateChanged;
    private BattleState _lastNotifiedBattleState = BattleState.inProgress;
    public BattleState BattleState
    {
        get
        {
            var aliveTeams = CombatUnits.Select(u => u.IsBlueTeam).Distinct().ToList();
            if (aliveTeams.Count <= 1)
            {
                var state = aliveTeams.Count == 1 && aliveTeams[0] ? BattleState.blueTeamWins : BattleState.redTeamWins;
                return state;
            }
            return BattleState.inProgress;
        }
    }
    private void UpdateBattleStateAndNotifyIfNeeded()
    {
        var current = BattleState;
        if (current != _lastNotifiedBattleState)
        {
            // Если состояние перешло в финальное — оповещаем
            if (current == BattleState.blueTeamWins || current == BattleState.redTeamWins)
            {
                _lastNotifiedBattleState = current;
                BattleStateChanged?.Invoke(current);
            }
            else
            {
                // Если вернулись в inProgress (редкий кейс) — обновляем tracking
                _lastNotifiedBattleState = current;
            }
        }
    }
    public void ConfigureLocalSide(bool isHostBlueTeam, Material myTeamMaterial = null)
    {
        MyIsBlueTeam = isHostBlueTeam;
        MyTeamMaterial = myTeamMaterial;
    }
    public virtual void AddCombatUnit(ICombatObject unit)
    {
        if (unit == null) return;
        if (CombatUnits.Contains(unit)) return;
        UpdateBattleStateAndNotifyIfNeeded();
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
        UpdateBattleStateAndNotifyIfNeeded();
    }
    public void ClearUnits()
    {
        CombatUnits.Clear();
        turnDict.Clear();
        TurnNumber.Value = 0;
        ActiveObject.Value = null;
    }
    public void RunBattle()
    {
        turnDict.Clear();
        TurnNumber.Value = 0;
        ActiveObject.Value = null;
        AddFirstUnits();
        UpdateBattleStateAndNotifyIfNeeded();
        TakeTurn();
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
