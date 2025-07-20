public interface ICombatUnit
{
    void TakeTurn();
    void EndTurn();
    bool IsBlueTeam { get; }
    UnitType  UnitType { get; }
}
