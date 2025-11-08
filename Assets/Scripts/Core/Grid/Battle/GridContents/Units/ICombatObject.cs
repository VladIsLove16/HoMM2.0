using System;

/// <summary>
/// Core combat unit contract used by both adventure and grid gameplay layers.
/// </summary>
public interface ICombatObject : IGridContent
{
    void TakeTurn();
    void EndTurn();
    UnitType UnitType { get; }
    UnitStats Stats { get; }
    Action<ICombatObject> Died { get; set; }
}
