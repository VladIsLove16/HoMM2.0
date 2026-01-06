using Adventure.Domain.Inventory;

namespace Adventure.Infrastructure.Inventory
{
    public interface IMushroomDropService
    {
        bool TrySpawn(UnitType type);
    }
}
