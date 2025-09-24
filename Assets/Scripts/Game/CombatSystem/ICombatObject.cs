using NUnit.Framework;
using System.Collections.Generic;

public interface ICombatObject : IGridContent
{
    void TakeTurn();
    void EndTurn();
    UnitType UnitType { get; }
    UnitStats Stats { get; }
}
