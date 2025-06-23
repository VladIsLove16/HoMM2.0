using System;
using System.Collections.Generic;
using static CombatSystem;

public interface ICombatAction
{
    void Execute(IDamageSource source, IDamageable target);
}
public interface ICombatUnit
{
    void TakeTurn();
    bool IsBlueTeam { get; }
    event Action<ICombatUnit> OnTurnEnded;
}
public enum BattleState
{
    inProgress,
    blueTeamWins,
    redTeamWins
}
public class CombatSystem
{
    public List<ICombatUnit> CombatUnits { get; } = new List<ICombatUnit>();
    public int TurnNumber { get; private set; } = 0;
    public ICombatUnit currentUnit;
    int turnTowards = 3;
    public void AddCombatUnit(ICombatUnit unit) => CombatUnits.Add(unit);

    private Dictionary<int, Queue<ICombatUnit>> turnQueue = new();
    public BattleState BattleInProgress()
    {
        bool team1;
        team1 = CombatUnits[0].IsBlueTeam;
        for(int i = 1; i< CombatUnits.Count; i++)
        {
            if(CombatUnits[i].IsBlueTeam != team1)
            {
                return BattleState.inProgress;
            }
        }
        return team1 == true ? BattleState.blueTeamWins: BattleState.redTeamWins;
    }
    private void AddNextTurnUnits(int turn)
    {
        foreach (var unit in CombatUnits)
        {
            turnQueue[turn].Enqueue(unit);
        }
    }
    public void RunBattle()
    {
        for (int i = 0; i < turnTowards; i++)
            AddNextTurnUnits(i);
        ICombatUnit combatUnit = DequeueUnit();
        combatUnit.OnTurnEnded += OnTurnEnded; 
        combatUnit.TakeTurn();
    }

    private void OnTurnEnded(ICombatUnit combatUnit)
    {
        combatUnit.OnTurnEnded -= OnTurnEnded;
        ICombatUnit nextUnit = DequeueUnit();
        nextUnit.OnTurnEnded += OnTurnEnded;
        nextUnit.TakeTurn();
    }

    private ICombatUnit DequeueUnit()
    {
        ICombatUnit combatUnit = turnQueue[0].Dequeue();
        if (turnQueue[0].Count == 0)
        {
            turnQueue.Remove(0);

        }
        return combatUnit;
    }
}
