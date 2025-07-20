public struct UnitTurnInfo
{
    public UnitTurnInfo(ICombatUnit combatUnit, int Turn)
    {
        this.Unit = combatUnit;
        this.Turn = Turn;
    }
    public readonly ICombatUnit Unit;
    public readonly int Turn;
}