public struct UnitTurnInfo
{
    public UnitTurnInfo(ICombatObject combatUnit, int Turn)
    {
        this.Unit = combatUnit;
        this.Turn = Turn;
    }
    public readonly ICombatObject Unit;
    public readonly int Turn;
}