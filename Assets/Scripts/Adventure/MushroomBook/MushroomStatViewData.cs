using UnityEngine;

namespace Adventure.Presentation.Mushroom
{
    public sealed class MushroomStatViewData
    {
        public MushroomStatViewData(UnitStatType type, string value, string label = null, Sprite icon = null)
        {
            Type = type;
            Label = string.IsNullOrEmpty(label) ? type.ToString() : label;
            Value = value ?? string.Empty;
            Icon = icon;
        }

        public UnitStatType Type { get; }
        public string Label { get; }
        public string Value { get; }
        public Sprite Icon { get; }
    }
}
