using UnityEngine;

namespace Adventure.Integration.Battle
{
    public readonly struct UnitStackData
    {
        public UnitStackData(UnitType unitType, int amount)
        {
            UnitType = unitType;
            Amount = Mathf.Max(1, amount);
        }

        public UnitType UnitType { get; }
        public int Amount { get; }
    }
}
