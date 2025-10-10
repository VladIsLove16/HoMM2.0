using System.Collections.Generic;

namespace Adventure.Domain.Inventory
{
    public interface IMushroomCatalog
    {
        bool TryGet(string id, out MushroomItem item);
        IReadOnlyList<MushroomItem> GetAll();
    }
}
