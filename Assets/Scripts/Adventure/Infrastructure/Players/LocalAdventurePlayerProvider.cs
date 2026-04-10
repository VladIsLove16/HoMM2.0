using System;
using Adventure.Infrastructure.Interaction;
using Adventure.Infrastructure.Movement;
using UnityEngine;

namespace Adventure.Infrastructure.Players
{
    public sealed class LocalAdventurePlayerProvider : ILocalAdventurePlayerProvider
    {
        public event Action PlayerChanged;

        public PlayerMovementController MovementController { get; private set; }
        public PlayerInteractionController InteractionController { get; private set; }
        public Transform PlayerTransform => MovementController != null ? MovementController.transform : null;
        public bool HasPlayer => MovementController != null;

        public void Register(PlayerMovementController movementController, PlayerInteractionController interactionController)
        {
            if (MovementController == movementController && InteractionController == interactionController)
                return;

            MovementController = movementController;
            InteractionController = interactionController;
            PlayerChanged?.Invoke();
        }

        public void Unregister(PlayerMovementController movementController)
        {
            if (movementController == null || MovementController != movementController)
                return;

            MovementController = null;
            InteractionController = null;
            PlayerChanged?.Invoke();
        }
    }
}
