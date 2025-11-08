using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Gameplay/Army Lineup", fileName = "ArmyLineup")]
public sealed class ArmyLineupSO : ScriptableObject
{
    [Serializable]
    private struct UnitStack
    {
        public UnitType UnitType;
        public int Amount;
    }

    [SerializeField] private List<UnitStack> playerUnits = new List<UnitStack>();

    public IReadOnlyList<UnitStackData> Convert()
    {
        return playerUnits
            .Select(stack => new UnitStackData(stack.UnitType, Mathf.Max(0, stack.Amount)))
            .ToList();
    }

    public static ArmyLineupSO CreateRuntimeLineup(IEnumerable<UnitStackData> stacks)
    {
        var lineup = CreateInstance<ArmyLineupSO>();
        lineup.hideFlags = HideFlags.HideAndDontSave;
        lineup.SetUnits(stacks);
        return lineup;
    }

    public void SetUnits(IEnumerable<UnitStackData> stacks)
    {
        playerUnits = stacks?
            .Select(stack => new UnitStack { UnitType = stack.UnitType, Amount = stack.Amount })
            .ToList() ?? new List<UnitStack>();
    }
}
