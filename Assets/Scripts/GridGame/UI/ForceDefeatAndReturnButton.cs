using Adventure.Infrastructure.State;
using CustomEventBus;
using Game.Events;
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
            SceneLoader.Load(targetScene);
        }
    }
}
