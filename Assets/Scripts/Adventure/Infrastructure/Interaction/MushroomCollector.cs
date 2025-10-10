using Adventure.Application.Inventory;
using Adventure.Infrastructure.Inventory;

namespace Adventure.Infrastructure.Interaction
{
    /// <summary>
    /// Application service that bridges collectible mushrooms with the inventory domain model.
    /// </summary>
    public sealed class MushroomCollector
    {
        private readonly MushroomBookViewModel _inventoryService;

        public MushroomCollector(MushroomBookViewModel inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public void Collect(IMushroomCollectible collectible, PlayerInteractionContext context)
        {
            if (collectible == null || !collectible.CanCollect)
                return;

            collectible.Collect(context);
            _inventoryService?.Add(collectible.Type);
        }
    }
}
