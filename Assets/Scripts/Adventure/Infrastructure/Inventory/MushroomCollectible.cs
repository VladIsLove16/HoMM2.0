using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Interaction;
using UnityEngine;
namespace Adventure.Infrastructure.Inventory
{
    /// <summary>
    /// Runtime behaviour for mushroom pickups encountered in the world.
    /// </summary>
    public sealed class MushroomCollectible : MonoBehaviour, IMushroomCollectible
    {
        [SerializeField] private UnitType UnitType = UnitType.Archer;
        [SerializeField] private bool destroyOnCollect = true;
        public UnitType Type => UnitType;
        public bool CanCollect { get; private set; } = true;

        public void Interact(PlayerInteractionContext context)
        {
            Collect(context);
        }

        public void Collect(PlayerInteractionContext context)
        {
            if (!CanCollect)
                return;

            CanCollect = false;

            if (destroyOnCollect && gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        public string GetPrompt()
        {
            return "Collect " + UnitType.ToString();
        }
    }
}
