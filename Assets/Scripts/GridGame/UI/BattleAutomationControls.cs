using System;
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

        private IBattleControlModeService _battleControlModes;

        [Inject]
        public void Construct(IBattleControlModeService battleControlModes)
        {
            _battleControlModes = battleControlModes ?? throw new ArgumentNullException(nameof(battleControlModes));
        }

        private void Awake()
        {
            Bind(enemyManualButton, SetEnemyManual);
            Bind(enemyAiButton, SetEnemyAi);
            Bind(localManualButton, SetLocalManual);
            Bind(localAiButton, SetLocalAi);
            Bind(fastResolveButton, FastResolveBattle);
        }

        private void OnDestroy()
        {
            Unbind(enemyManualButton, SetEnemyManual);
            Unbind(enemyAiButton, SetEnemyAi);
            Unbind(localManualButton, SetLocalManual);
            Unbind(localAiButton, SetLocalAi);
            Unbind(fastResolveButton, FastResolveBattle);
        }

        public void SetEnemyManual() => _battleControlModes?.SetEnemyTeamsMode(BattleControlMode.Manual);
        public void SetEnemyAi() => _battleControlModes?.SetEnemyTeamsMode(BattleControlMode.AI);
        public void SetLocalManual() => _battleControlModes?.SetLocalTeamMode(BattleControlMode.Manual);
        public void SetLocalAi() => _battleControlModes?.SetLocalTeamMode(BattleControlMode.AI);
        public void FastResolveBattle() => _battleControlModes?.EnableFastResolve();

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
