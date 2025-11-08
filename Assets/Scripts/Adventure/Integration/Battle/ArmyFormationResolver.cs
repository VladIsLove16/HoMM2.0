using System;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Integration.Battle
{
    /// <summary>
    /// Produces simple grid formations for player and enemy armies.
    /// </summary>
    public sealed class ArmyFormationResolver
    {
        private readonly int _playerFrontlineY;
        private readonly int _enemyFrontlineY;
        private readonly int _columnSpacing;

        public ArmyFormationResolver(int playerFrontlineY, int enemyFrontlineY, int columnSpacing)
        {
            _playerFrontlineY = playerFrontlineY;
            _enemyFrontlineY = enemyFrontlineY;
            _columnSpacing = Mathf.Max(1, columnSpacing);
        }

        public IEnumerable<GridContentEntrySO.UnitContent> CreatePlayerFormation(
            IReadOnlyList<UnitStackData> units,
            int gridWidth)
        {
            return CreateFormation(units, gridWidth, _playerFrontlineY, Team.Blue);
        }

        public IEnumerable<GridContentEntrySO.UnitContent> CreateEnemyFormation(
            IReadOnlyList<UnitStackData> units,
            int gridWidth)
        {
            return CreateFormation(units, gridWidth, _enemyFrontlineY, Team.Red);
        }

        private IEnumerable<GridContentEntrySO.UnitContent> CreateFormation(
            IReadOnlyList<UnitStackData> stacks,
            int gridWidth,
            int frontlineY,
            Team team)
        {
            if (stacks == null || stacks.Count == 0)
                yield break;

            var x = 0;
            var spacing = Mathf.Max(1, _columnSpacing);
            var currentRow = frontlineY;

            foreach (var stack in stacks)
            {
                if (stack.Amount <= 0)
                    continue;

                var slot = new GridContentEntrySO.UnitContent
                {
                    Team = team,
                    unitType = stack.UnitType,
                    Amount = Mathf.Max(1, stack.Amount),
                    X = Mathf.Clamp(x, 0, Mathf.Max(0, gridWidth - 1)),
                    Y = currentRow
                };

                yield return slot;

                x += spacing;
                if (gridWidth > 0 && x >= gridWidth)
                {
                    x = x % gridWidth;
                    currentRow += team == Team.Blue ? 1 : -1;
                }
            }
        }
    }
}
