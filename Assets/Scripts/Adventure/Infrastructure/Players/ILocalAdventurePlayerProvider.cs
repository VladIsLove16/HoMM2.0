using System;
using Adventure.Infrastructure.Interaction;
using Adventure.Infrastructure.Movement;
using UnityEngine;

namespace Adventure.Infrastructure.Players
{
    public interface ILocalAdventurePlayerProvider
    {
        event Action PlayerChanged;

        PlayerMovementController MovementController { get; }
        PlayerInteractionController InteractionController { get; }
        Transform PlayerTransform { get; }
        bool HasPlayer { get; }

        void Register(PlayerMovementController movementController, PlayerInteractionController interactionController);
        void Unregister(PlayerMovementController movementController);
    }
}
