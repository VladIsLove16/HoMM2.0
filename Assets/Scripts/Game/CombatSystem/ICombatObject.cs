using NUnit.Framework;
using System.Collections.Generic;

public interface ICombatObject : IGridContent
{
    void TakeTurn();
    void EndTurn();
    bool IsBlueTeam { get; }
    UnitType UnitType { get; }
    UnitStats Stats { get; }
}
