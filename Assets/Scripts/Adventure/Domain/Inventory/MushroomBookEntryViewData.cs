using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Domain.Inventory
{
    public readonly struct MushroomBookEntryViewData
    {
        public MushroomBookEntryViewData(
            string name,
            string description,
            Sprite icon,
            Sprite hoveredIcon,
            Sprite humanizedIcon,
            Sprite humanizedHoveredIcon,
            IReadOnlyList<string> characteristics)
        {
            Name = name;
            Description = description;
            Icon = icon;
            HoveredIcon = hoveredIcon;
            HumanizedIcon = humanizedIcon;
            HumanizedHoveredIcon = humanizedHoveredIcon;
            Characteristics = characteristics;
        }

        public string Name { get; }
        public string Description { get; }
        public Sprite Icon { get; }
        public Sprite HoveredIcon { get; }
        public Sprite HumanizedIcon { get; }
        public Sprite HumanizedHoveredIcon { get; }
        public IReadOnlyList<string> Characteristics { get; }

        public static MushroomBookEntryViewData Empty => new MushroomBookEntryViewData(string.Empty, string.Empty, null, null, null, null, new List<string>());
    }
}
