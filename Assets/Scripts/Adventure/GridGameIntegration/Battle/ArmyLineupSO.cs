using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Integration.Battle
{
    [CreateAssetMenu(menuName = "Adventure/Army Lineup", fileName = "ArmyLineup")]
    public sealed class ArmyLineupSO : ScriptableObject
    {
        [System.Serializable]
        private struct UnitStack
        {
            public UnitType UnitType;
            public int Amount;
        }

        [SerializeField] private List<UnitStack> playerUnits = new List<UnitStack>();
        [SerializeField] private List<UnitStack> enemyUnits = new List<UnitStack>();

        public IReadOnlyList<UnitStackData> GetPlayerLineup() => Convert(playerUnits);
        public IReadOnlyList<UnitStackData> GetEnemyLineup() => Convert(enemyUnits);

        private static List<UnitStackData> Convert(List<UnitStack> source)
        {
            var result = new List<UnitStackData>(source.Count);
            foreach (var stack in source)
            {
                result.Add(new UnitStackData(stack.UnitType, stack.Amount));
            }
            return result;
        }
    }
}
