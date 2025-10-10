using System.Collections.Generic;
using Adventure.Domain.Inventory;
using UnityEngine;

namespace Adventure.Integration.Battle
{
    [CreateAssetMenu(menuName = "Adventure/Battle/NPC Army", fileName = "NpcArmy")]
    public sealed class NpcArmyDefinitionSO : ScriptableObject
    {
        [System.Serializable]
        private struct MushroomStack
        {
            public UnitType UnitType;
            [Min(1)] public int Amount;
        }

        [SerializeField] private List<MushroomStack> enemyUnits = new List<MushroomStack>();

        public IEnumerable<MushroomArmyEntry> EnumerateEntries()
        {
            foreach (var stack in enemyUnits)
            {
                if (stack.UnitType == UnitType.Archer || stack.Amount <= 0)
                    continue;

                yield return new MushroomArmyEntry(stack.UnitType, stack.Amount);
            }
        }
    }

    public readonly struct MushroomArmyEntry
    {
        public MushroomArmyEntry(UnitType UnitType, int amount)
        {
            this.UnitType = UnitType;
            Amount = amount;
        }

        public UnitType UnitType { get; }
        public int Amount { get; }
    }
}
