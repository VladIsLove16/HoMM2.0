using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

public interface IUnitStatsProvider
{
    public IEnumerable<UnitType> Types { get; }
    bool TryGetBaseStats(UnitType type, out UnitStats stats);
}
