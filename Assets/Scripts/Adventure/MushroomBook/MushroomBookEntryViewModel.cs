using System;
using System.Collections.Generic;
using Adventure.Domain.Inventory;
using UnityEngine;

namespace Adventure.Presentation.Mushroom
{

    public sealed class MushroomBookEntryViewModel
    {
        public static readonly MushroomBookEntryViewModel Empty = new MushroomBookEntryViewModel(
            default,
            string.Empty,
            string.Empty,
            null,
            null,
            null,
            null,
            Array.Empty<MushroomStatViewData>());

        public MushroomBookEntryViewModel(
            UnitType unitType,
            string displayName,
            string description,
            Sprite icon,
            Sprite hoveredIcon,
            Sprite humanizedIcon,
            Sprite humanizedHoveredIcon,
            IReadOnlyList<MushroomStatViewData> stats)
        {
            UnitType = unitType;
            DisplayName = displayName ?? string.Empty;
            Description = description ?? string.Empty;
            Icon = icon;
            HoveredIcon = hoveredIcon;
            HumanizedIcon = humanizedIcon;
            HumanizedHoveredIcon = humanizedHoveredIcon;
            Stats = stats ?? Array.Empty<MushroomStatViewData>();
        }

        public UnitType UnitType { get; }
        public string DisplayName { get; }
        public int Amount { get; set; }
        public string Description { get; }
        public Sprite Icon { get; }
        public Sprite HoveredIcon { get; }
        public Sprite HumanizedIcon { get; }
        public Sprite HumanizedHoveredIcon { get; }
        public IReadOnlyList<MushroomStatViewData> Stats { get; }

        public Sprite GetSprite(PresentationMode mode, bool hovered)
        {
            if (hovered)
            {
                return mode == PresentationMode.Humanized
                    ? FirstNotNull(HumanizedHoveredIcon, HumanizedIcon, HoveredIcon, Icon)
                    : FirstNotNull(HoveredIcon, Icon, HumanizedHoveredIcon, HumanizedIcon);
            }

            return mode == PresentationMode.Humanized
                ? FirstNotNull(HumanizedIcon, Icon, HumanizedHoveredIcon, HoveredIcon)
                : FirstNotNull(Icon, HoveredIcon, HumanizedIcon, HumanizedHoveredIcon);
        }

        private static Sprite FirstNotNull(params Sprite[] sprites)
        {
            foreach (var sprite in sprites)
            {
                if (sprite != null)
                {
                    return sprite;
                }
            }

            return null;
        }
    }
}
