using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Inventory;
using System;

namespace Adventure.Infrastructure.Interaction
{
    /// <summary>
    /// Application service that bridges collectible mushrooms with the inventory domain model.
    /// </summary>
    public sealed class MushroomCollectionViewModel
    {
        private readonly MushroomInventoryModel _mushroomInventoryModel;

        public MushroomCollectionViewModel(MushroomInventoryModel mushroomInventoryModel)
        {
            _mushroomInventoryModel = mushroomInventoryModel;
        }
        public void Collect(UnitType type)
        {
            _mushroomInventoryModel?.Add(type);
        }
    }
}
