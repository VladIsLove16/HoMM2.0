using Adventure.Application.Inventory;
using Adventure.Infrastructure.Movement;
using UnityEngine;

namespace Adventure.Infrastructure.Interaction
{
    public sealed class PlayerInteractionController : MonoBehaviour
    {
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactDistance = 2f;
        [SerializeField] private LayerMask interactionMask = ~0;

        private MushroomCollector _mushroomCollector;

        public void Construct(MushroomInventoryService inventoryService)
        {
            _mushroomCollector = new MushroomCollector(inventoryService);
        }

        private void OnEnable()
        {
            if (movementController != null)
            {
                movementController.Interact += HandleInteract;
                movementController.Collect += HandleCollect;
            }
        }

        private void OnDisable()
        {
            if (movementController != null)
            {
                movementController.Interact -= HandleInteract;
                movementController.Collect -= HandleCollect;
            }
        }

        private void HandleInteract()
        {
            if (playerCamera == null)
                return;

            var origin = playerCamera.transform.position;
            var direction = playerCamera.transform.forward;
            if (Physics.Raycast(origin, direction, out var hit, interactDistance, interactionMask))
            {
                var interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact(new PlayerInteractionContext(transform));
                }
            }
        }

        private void HandleCollect()
        {
            if (playerCamera == null)
                return;

            var origin = playerCamera.transform.position;
            var direction = playerCamera.transform.forward;
            if (Physics.Raycast(origin, direction, out var hit, interactDistance, interactionMask))
            {
                var collectible = hit.collider.GetComponentInParent<ICollectible>();
                if (collectible != null && collectible.CanCollect)
                {
                    collectible.Collect();
                    _mushroomCollector?.ReportCollected(collectible as IMushroomCollectible);
                }
            }
        }
    }

    public interface IMushroomCollectible : ICollectible
    {
        string MushroomId { get; }
    }

    internal sealed class MushroomCollector
    {
        private readonly MushroomInventoryService _inventoryService;

        public MushroomCollector(MushroomInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public void ReportCollected(IMushroomCollectible collectible)
        {
            if (collectible == null)
                return;

            _inventoryService?.Add(collectible.MushroomId);
        }
    }
}
