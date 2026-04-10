using Adventure.Infrastructure.Interaction;
using Adventure.Infrastructure.Movement;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Adventure.Infrastructure.Players
{
    public sealed class SceneLocalAdventurePlayerRegistrar : IInitializable, System.IDisposable
    {
        private readonly ILocalAdventurePlayerProvider _provider;
        private readonly PlayerMovementController _sceneMovementController;
        private readonly PlayerInteractionController _sceneInteractionController;

        public SceneLocalAdventurePlayerRegistrar(
            ILocalAdventurePlayerProvider provider,
            PlayerMovementController sceneMovementController,
            PlayerInteractionController sceneInteractionController)
        {
            _provider = provider;
            _sceneMovementController = sceneMovementController;
            _sceneInteractionController = sceneInteractionController;
        }

        public void Initialize()
        {
            if (_sceneMovementController == null)
                return;

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                DisableScenePlayer();
                return;
            }

            _provider.Register(_sceneMovementController, _sceneInteractionController);
        }

        public void Dispose()
        {
            _provider.Unregister(_sceneMovementController);
        }

        private void DisableScenePlayer()
        {
            var movementGo = _sceneMovementController != null ? _sceneMovementController.gameObject : null;
            var interactionGo = _sceneInteractionController != null ? _sceneInteractionController.gameObject : null;

            if (movementGo != null)
                movementGo.SetActive(false);

            if (interactionGo != null && interactionGo != movementGo)
                interactionGo.SetActive(false);
        }
    }
}
