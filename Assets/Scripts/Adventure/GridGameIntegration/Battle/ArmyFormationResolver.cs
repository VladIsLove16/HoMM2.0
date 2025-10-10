using System.Collections.Generic;

namespace Adventure.Integration.Battle
{
    public sealed class ArmyFormationResolver
    {
        private readonly int _playerFrontlineX;
        private readonly int _enemyFrontlineX;
        private readonly int _rowSpacing;

        public ArmyFormationResolver(int playerFrontlineX, int enemyFrontlineX, int rowSpacing)
        {
            _playerFrontlineX = playerFrontlineX;
            _enemyFrontlineX = enemyFrontlineX;
            _rowSpacing = rowSpacing;
        }

        public IReadOnlyList<GridSlot> ResolveForPlayer(IReadOnlyList<UnitStackData> lineup) => Resolve(lineup, _playerFrontlineX, Team.Blue);
        public IReadOnlyList<GridSlot> ResolveForEnemy(IReadOnlyList<UnitStackData> lineup) => Resolve(lineup, _enemyFrontlineX, Team.Red);

        private IReadOnlyList<GridSlot> Resolve(IReadOnlyList<UnitStackData> lineup, int baseX, Team team)
        {
            var result = new List<GridSlot>(lineup.Count);
            var y = 0;
            foreach (var stack in lineup)
            {
                result.Add(new GridSlot(baseX, y, stack.UnitType, stack.Amount, team));
                y += _rowSpacing;
            }
            return result;
        }
    }

    public readonly struct GridSlot
    {
        public GridSlot(int x, int y, UnitType unitType, int amount, Team team)
        {
            X = x;
            Y = y;
            UnitType = unitType;
            Amount = amount;
            Team = team;
        }

        public int X { get; }
        public int Y { get; }
        public UnitType UnitType { get; }
        public int Amount { get; }
        public Team Team { get; }
    }
}
