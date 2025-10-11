using System;
using System.Collections.Generic;

namespace Adventure.Integration.Battle
{
    public readonly struct BattleArmies
    {
        public BattleArmies(IReadOnlyList<UnitStackData> playerUnits, IReadOnlyList<UnitStackData> enemyUnits)
        {
            PlayerUnits = playerUnits ?? Array.Empty<UnitStackData>();
            EnemyUnits = enemyUnits ?? Array.Empty<UnitStackData>();
        }

        public IReadOnlyList<UnitStackData> PlayerUnits { get; }
        public IReadOnlyList<UnitStackData> EnemyUnits { get; }
    }
}
