using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
public interface ICombatAction
{
    void Execute(IDamageSource source, IDamagable target);
}
public interface ICombatUnit
{
    void TakeTurn();
    void EndTurn();
    bool IsBlueTeam { get; }
    UnitType  UnitType { get; }
}
public enum BattleState
{
    none,
    inProgress,
    blueTeamWins,
    redTeamWins
}
public class TurnSystem
{
    public List<ICombatUnit> CombatUnits { get; } = new List<ICombatUnit>();
    public int TurnNumber { get;private set; } = 0;
    public Action<int> TurnNumberChanged;
    public ICombatUnit currentUnit;
    int turnTowards = 3;
    public Action ActiveUnitChanged;
    public Action<UnitTurnInfo> CombatUnitsAdded;
    private Dictionary<int, List<ICombatUnit>> turnDict = new();
    private BattleState BattleState = BattleState.none;
    public void AddCombatUnit(ICombatUnit unit)
    {
        CombatUnits.Add(unit);
    }
    internal void RemoveCombatUnit(UnitModel unit)
    {
        CombatUnits.Remove(unit);
    }
    internal void ClearUnits()
    {
        CombatUnits.Clear();
    }
    public void RunBattle()
    {
        AddFirstUnits();
        TakeFirstTurn();
    }

    private void TakeFirstTurn()
    {
        BattleState = BattleState.inProgress;
        ICombatUnit combatUnit = DequeueUnit();
        currentUnit = combatUnit;
        combatUnit.TakeTurn();
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

    public BattleState UpdateBattleState()
    {
        bool team1;
        team1 = CombatUnits[TurnNumber].IsBlueTeam;
        for(int i = 1; i< CombatUnits.Count; i++)
        {
            if(CombatUnits[i].IsBlueTeam != team1)
            {
                return BattleState.inProgress;
            }
        }
        BattleState = team1 == true ? BattleState.blueTeamWins : BattleState.redTeamWins;
        return BattleState;
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


    private void AddUnitToTurn(int turn, ICombatUnit unit)
    {
        if (!turnDict.ContainsKey(turn))
        {
            turnDict[turn] = new();
        }
        turnDict[turn].Add(unit);
        CombatUnitsAdded?.Invoke(new UnitTurnInfo(unit, turn));
    }

    internal void EndTurn()
    {
        currentUnit.EndTurn();
        ICombatUnit nextUnit = DequeueUnit();
        currentUnit = nextUnit;
        nextUnit.TakeTurn();
        ActiveUnitChanged?.Invoke();
    }
    private ICombatUnit DequeueUnit()
    {
        if (turnDict[TurnNumber].Count == 0)
        {
            Debug.Log("unit dequied,no left units");
            turnDict.Remove(TurnNumber);
            TurnNumber++;
            TurnNumberChanged.Invoke(TurnNumber);
        }
        else
        {
            string a = string.Empty;
            foreach(var b in turnDict[TurnNumber])
            {
                a += b.ToString();
            }
            Debug.Log("unit dequied,left units" + a);

        }
        if(turnDict.Count<=TurnNumber+turnTowards)
        {
            AddUnitsUntil(TurnNumber + turnTowards);
        }
        ICombatUnit combatUnit = turnDict[TurnNumber][0];
        turnDict[TurnNumber].Remove(combatUnit);
        return combatUnit;
    }

}
