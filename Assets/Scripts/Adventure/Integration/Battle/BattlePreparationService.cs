using System;
using System.Collections.Generic;
using Adventure.Domain.Inventory;

namespace Adventure.Integration.Battle
{
    public sealed class BattlePreparationService
    {
        private readonly MushroomInventoryModel _inventoryService;
        private readonly UnitDefinitionSOCollection _catalog;
        private ArmyLineupSO _currentEnemy;

        public BattlePreparationService(MushroomInventoryModel inventoryService, UnitDefinitionSOCollection catalog)
        {
            _inventoryService = inventoryService;
            _catalog = catalog;
        }

        public void SetEnemyArmy(ArmyLineupSO armyDefinition)
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
            var result = new List<UnitStackData>(_inventoryService.Items.Count);
            foreach (var entry in _inventoryService.Items)
            {
                if (entry.Value <= 0)
                    continue;

                result.Add(new UnitStackData(entry.Key, entry.Value));
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
}
