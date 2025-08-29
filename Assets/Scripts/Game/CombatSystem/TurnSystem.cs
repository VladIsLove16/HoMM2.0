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
    int turnTowards = 3;
    public Action<UnitTurnInfo> CombatUnitsAdded;
    protected Dictionary<int, List<ICombatObject>> turnDict = new();
    private BattleState BattleState = BattleState.none;
    public virtual void AddCombatUnit(ICombatObject unit)
    {
        CombatUnits.Add(unit);
    }
    internal void RemoveCombatUnit(ICombatObject unit)
    {
        CombatUnits.Remove(unit);
    }
    internal void ClearUnits()
    {
        CombatUnits.Clear();
    }
    public void RunBattle()
    {
        BattleState = BattleState.inProgress;
        AddFirstUnits();
        TakeTurn();
    }
    public BattleState UpdateBattleState()
    {
        bool team1;
        team1 = CombatUnits[TurnNumber.Value].IsBlueTeam;
        for (int i = 1; i < CombatUnits.Count; i++)
        {
            if (CombatUnits[i].IsBlueTeam != team1)
            {
                return BattleState.inProgress;
            }
        }
        BattleState = team1 == true ? BattleState.blueTeamWins : BattleState.redTeamWins;
        return BattleState;
    }
    protected virtual ICombatObject GetFirstObject()
    {
        return turnDict[TurnNumber.Value][0];
    }
    public void TakeTurn()
    {
       
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
        ActiveObject.Value.EndTurn();
        ICombatObject nextUnit = DequeueUnit();
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
        turnDict[turn].Add(unit);
        CombatUnitsAdded?.Invoke(new UnitTurnInfo(unit, turn));
    }

    protected virtual ICombatObject DequeueUnit()
    {
        if (turnDict[TurnNumber.Value].Count == 0)
        {
            OnNoTurnUnitsLeft();
        }
        turnDict[TurnNumber.Value].Remove(ActiveObject.Value);
        if (turnDict[TurnNumber.Value].Count == 0)
        {
            OnNoTurnUnitsLeft();
        }
        ICombatObject combatUnit = turnDict[TurnNumber.Value][0];
        return combatUnit;
    }

    private void OnNoTurnUnitsLeft()
    {
        turnDict.Remove(TurnNumber.Value);
        TurnNumber.SetValueAndForceNotify(TurnNumber.Value + 1);
        if (turnDict.Count <= TurnNumber.Value + turnTowards)
        {
            AddUnitsUntil(TurnNumber.Value + turnTowards);
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
        foreach (var b in turnDict[TurnNumber.Value])
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
