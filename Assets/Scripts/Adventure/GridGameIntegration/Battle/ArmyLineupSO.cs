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

        public IReadOnlyList<UnitStackData> Convert() => Convert(playerUnits);

        public static ArmyLineupSO CreateRuntimeLineup(IEnumerable<UnitStackData> stacks)
        {
            var lineup = CreateInstance<ArmyLineupSO>();
            lineup.hideFlags = HideFlags.HideAndDontSave;
            lineup.SetPlayerUnits(stacks);
            return lineup;
        }

        public void SetPlayerUnits(IEnumerable<UnitStackData> stacks)
        {
            if (playerUnits == null)
            {
                playerUnits = new List<UnitStack>();
            }
            else
            {
                playerUnits.Clear();
            }

            if (stacks == null)
            {
                return;
            }

            foreach (var stack in stacks)
            {
                if (stack.Amount <= 0)
                {
                    continue;
                }

                playerUnits.Add(new UnitStack
                {
                    UnitType = stack.UnitType,
                    Amount = stack.Amount
                });
            }
        }

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
