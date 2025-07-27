using NUnit.Framework;
using System.Collections.Generic;

public interface ICombatObject
{
    void TakeTurn();
    void EndTurn();
    bool IsBlueTeam { get; }
    UnitType  UnitType { get; }
    List<IActionHandler> GetAvailableActions();
}
