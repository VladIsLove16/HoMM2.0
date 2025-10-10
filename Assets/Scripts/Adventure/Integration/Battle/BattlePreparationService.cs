using System;
using System.Collections.Generic;
using Adventure.Application.Inventory;
using Adventure.Domain.Inventory;

namespace Adventure.Integration.Battle
{
    public sealed class BattlePreparationService
    {
        private readonly MushroomBookViewModel _inventoryService;
        private readonly IMushroomCatalog _catalog;
        private NpcArmyDefinitionSO _currentEnemy;

        public BattlePreparationService(MushroomBookViewModel inventoryService, IMushroomCatalog catalog)
        {
            _inventoryService = inventoryService;
            _catalog = catalog;
        }

        public void SetEnemyArmy(NpcArmyDefinitionSO armyDefinition)
        {
            _currentEnemy = armyDefinition;
        }

        public BattleArmies BuildArmies()
        {
            var player = BuildPlayerLineup();
            var enemy = BuildEnemyLineup();
            return new BattleArmies(player, enemy);
        }

        private IReadOnlyList<UnitStackData> BuildPlayerLineup()
        {
            var result = new List<UnitStackData>(_inventoryService.Entries.Count);
            foreach (var entry in _inventoryService.Entries)
            {
                if (entry.Amount <= 0)
                    continue;

                result.Add(new UnitStackData(entry.Item.UnitType, entry.Amount));
            }

            return result;
        }

        private IReadOnlyList<UnitStackData> BuildEnemyLineup()
        {
            if (_currentEnemy == null)
                return Array.Empty<UnitStackData>();

            var result = new List<UnitStackData>();

            foreach (var stack in _currentEnemy.EnumerateEntries())
            {
                if (!_catalog.TryGet(stack.UnitType, out var item))
                    continue;

                result.Add(new UnitStackData(item.UnitType, stack.Amount));
            }

            return result;
        }
    }

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
