using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Domain.Inventory
{
    public readonly struct MushroomViewModel
    {
        public MushroomViewModel(
            UnitType id,
            string name,
            string description,
            Sprite icon,
            Sprite hoveredIcon,
            Sprite humanizedIcon,
            Sprite humanizedHoveredIcon,
            IReadOnlyList<string> characteristics)
        {
            Id = id;
            Name = name;
            Description = description;
            Icon = icon;
            HoveredIcon = hoveredIcon;
            HumanizedIcon = humanizedIcon;
            HumanizedHoveredIcon = humanizedHoveredIcon;
            Characteristics = characteristics;
        }

        public UnitType Id { get; }
        public string Name { get; }
        public string Description { get; }
        public Sprite Icon { get; }
        public Sprite HoveredIcon { get; }
        public Sprite HumanizedIcon { get; }
        public Sprite HumanizedHoveredIcon { get; }
        public IReadOnlyList<string> Characteristics { get; }

        public static MushroomViewModel Empty => new MushroomViewModel(UnitType.Archer, string.Empty, string.Empty, null, null, null, null, new List<string>());
    }
}
