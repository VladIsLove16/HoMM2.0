using UnityEngine;

namespace Adventure.Application.Rewards
{
    public readonly struct BattleRewardItemViewData
    {
        public BattleRewardItemViewData(UnitType unitType, int amount, string displayName, Sprite icon)
        {
            UnitType = unitType;
            Amount = amount;
            DisplayName = displayName;
            Icon = icon;
        }

        public UnitType UnitType { get; }
        public int Amount { get; }
        public string DisplayName { get; }
        public Sprite Icon { get; }
    }
}
