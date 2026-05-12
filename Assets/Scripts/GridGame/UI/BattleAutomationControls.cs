using System;
using Adventure.Infrastructure.State;
using CustomEventBus;
using Game.Events;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GridGame.UI
{
    public sealed class BattleAutomationControls : MonoBehaviour
    {
        [SerializeField] private Button enemyManualButton;
        [SerializeField] private Button enemyAiButton;
        [SerializeField] private Button localManualButton;
        [SerializeField] private Button localAiButton;
        [SerializeField] private Button fastResolveButton;
        [SerializeField] private Button forceVictoryButton;
        [SerializeField] private Button forceDefeatButton;

        private IBattleControlModeService _battleControlModes;
        private EventBus _eventBus;
        private SinglePlayerStartConfigurationSO _startConfiguration;

        [Inject]
        public void Construct(
            IBattleControlModeService battleControlModes,
            EventBus eventBus,
            [InjectOptional] SinglePlayerStartConfigurationSO startConfiguration = null)
        {
            _battleControlModes = battleControlModes ?? throw new ArgumentNullException(nameof(battleControlModes));
            _eventBus = eventBus;
            _startConfiguration = startConfiguration;
        }

        private void Awake()
        {
            Bind(enemyManualButton, SetEnemyManual);
            Bind(enemyAiButton, SetEnemyAi);
            Bind(localManualButton, SetLocalManual);
            Bind(localAiButton, SetLocalAi);
            Bind(fastResolveButton, FastResolveBattle);
            Bind(forceVictoryButton, ForceVictoryAndReturn);
            Bind(forceDefeatButton, ForceDefeatAndReturn);
        }

        private void OnDestroy()
        {
            Unbind(enemyManualButton, SetEnemyManual);
            Unbind(enemyAiButton, SetEnemyAi);
            Unbind(localManualButton, SetLocalManual);
            Unbind(localAiButton, SetLocalAi);
            Unbind(fastResolveButton, FastResolveBattle);
            Unbind(forceVictoryButton, ForceVictoryAndReturn);
            Unbind(forceDefeatButton, ForceDefeatAndReturn);
        }

        public void SetEnemyManual() => _battleControlModes?.SetEnemyTeamsMode(BattleControlMode.Manual);
        public void SetEnemyAi() => _battleControlModes?.SetEnemyTeamsMode(BattleControlMode.AI);
        public void SetLocalManual() => _battleControlModes?.SetLocalTeamMode(BattleControlMode.Manual);
        public void SetLocalAi() => _battleControlModes?.SetLocalTeamMode(BattleControlMode.AI);
        public void FastResolveBattle() => _battleControlModes?.EnableFastResolve();
        public void ForceVictoryAndReturn() => CompleteBattleAndReturn(playerWon: true);
        public void ForceDefeatAndReturn() => CompleteBattleAndReturn(playerWon: false);

        private void CompleteBattleAndReturn(bool playerWon)
        {
            _startConfiguration?.EnsureDirectBattlePostBattleContext();
            BattleStateCache.CompleteBattle(playerWon);
            _eventBus?.Invoke(new BattleCompletedCustomEvent(playerWon));

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

                Debug.LogWarning("[BattleAutomationControls] Waiting for the host to return the party to the adventure scene.", this);
                return;
            }

            SceneLoader.Load(targetScene);
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
                button.onClick.AddListener(action);
        }

        private static void Unbind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
                button.onClick.RemoveListener(action);
        }
    }
}
