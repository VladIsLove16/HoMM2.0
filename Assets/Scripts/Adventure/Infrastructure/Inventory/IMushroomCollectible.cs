using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Interaction;

namespace Adventure.Infrastructure.Inventory
{
    /// <summary>
    /// Contract for mushroom pickups, allowing the interaction layer
    /// to operate on abstractions instead of concrete MonoBehaviours.
    /// </summary>
    public interface IMushroomCollectible : ICollectible
    {
        /// <summary>
        /// Identifier of the mushroom item that should be added to inventory.
        /// </summary>
        UnitType Type { get; }
    }
}
