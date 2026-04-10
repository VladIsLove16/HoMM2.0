using Adventure.Infrastructure.State;
using CustomEventBus;
using Game.Events;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GridGame.UI
{
    /// <summary>
    /// UI helper to forcibly end the current battle as a defeat and return to the adventure scene.
    /// Attach to a button on the grid scene.
    /// </summary>
    public sealed class ForceDefeatAndReturnButton : MonoBehaviour
    {
        [SerializeField] private Button button;

        private EventBus _eventBus;

        [Inject]
        public void Construct(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(OnClicked);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClicked);
            }
        }

        private void OnClicked()
        {
            // Mark battle as a player defeat and route back to the adventure scene.
            BattleStateCache.CompleteBattle(playerWon: false);
            _eventBus?.Invoke(new BattleCompletedCustomEvent(false));
            var targetScene = BattleStateCache.GetReturnSceneOrDefault();
            var networkManager = NetworkManager.Singleton;
            if (networkManager != null && networkManager.IsListening)
            {
                if (networkManager.IsServer || networkManager.IsHost)
                {
                    networkManager.SceneManager.LoadScene(targetScene.ToString(), UnityEngine.SceneManagement.LoadSceneMode.Single);
                    return;
                }

#if UNITY_2023_1_OR_NEWER
                var gateway = UnityEngine.Object.FindFirstObjectByType<GameNetworkCommandGateway>();
#else
                var gateway = UnityEngine.Object.FindObjectOfType<GameNetworkCommandGateway>();
#endif
                if (gateway != null && gateway.RequestReturnToAdventure())
                    return;

                Debug.LogWarning("[ForceDefeatAndReturnButton] Waiting for the host to return the party to the adventure scene.", this);
                return;
            }

            SceneLoader.Load(targetScene);
        }
    }
}
