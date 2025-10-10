using System.Collections.Generic;

namespace Adventure.Domain.Inventory
{
    public interface IMushroomCatalog
    {
        bool TryGet(UnitType type, out MushroomModel item);
        IReadOnlyList<MushroomModel> GetAll();
        bool TryGetVisuals(UnitType type, out MushroomViewModel visuals);
    }
}
