using Adventure.Integration.Battle;
using System;
using System.Collections.Generic;
using System.Text;

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
            if(playerFrontlineX == enemyFrontlineX)
            {
                throw new ArgumentException("playerFrontlineX cant be equal enemyFrontlineX. Value: " + playerFrontlineX);
            }
        }

        public ArmyFormation ResolveForPlayer(IReadOnlyList<UnitStackData> lineup) => Resolve(lineup, _playerFrontlineX, Team.Blue);
        public ArmyFormation ResolveForEnemy(IReadOnlyList<UnitStackData> lineup) => Resolve(lineup, _enemyFrontlineX, Team.Red);

        private ArmyFormation Resolve(IReadOnlyList<UnitStackData> lineup, int baseX, Team team)
        {
            if(lineup == null || lineup.Count == 0)
                throw new ArgumentException("lineup is null or empty");
            var result = new ArmyFormation();
            var y = 0;
            foreach (var stack in lineup)
            {
                result.Add(new GridSlot(baseX, y, stack.UnitType, stack.Amount, team));
                y += _rowSpacing;
            }
            return result;
        }
    }
}
public class ArmyFormation
{
    private List<GridSlot> gridSlots = new();
    public int Count => gridSlots.Count;
    public ArmyFormation() {  }

    public void Add(GridSlot gridSlot)
    {
        gridSlots.Add(gridSlot);
    }
    public GridSlot this[int index]
    {
        get => gridSlots[index];
    }
    public new string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var slot in gridSlots)
        {
            stringBuilder.Append(slot.ToString());
        }
        return stringBuilder.ToString();
    }
}