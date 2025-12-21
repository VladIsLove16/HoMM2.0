using Adventure.Infrastructure.State;
using Unity.Behavior;
using UnityEngine;
using static Adventure.Infrastructure.Dialog.NpcBattleAnimationController;

namespace Adventure.Infrastructure.Dialog
{
    [DisallowMultipleComponent]
    public sealed class NpcBehaviorGraphBridge : MonoBehaviour
    {
        private static class BlackboardKeys
        {
            public const string PlayerTransform = "PlayerTransform";
            public const string LookDistance = "LookDistance";
            public const string IsPlayerInDialog = "IsPlayerInDialog";
            public const string DialogueChoiceOpened = "DialogueChoiceOpened";
            public const string BattleRequested = "BattleRequested";
            public const string BattleOutcome = "New BattleOutcome";
            public const string DialogueEventChannel = "New DialogueEventChannel";
            public const string ChoiceEventChannel = "New ChoiceEventChannel";
            public const string BattleResultChannel = "New BattleResultChannel";
        }

        [SerializeField] private BehaviorGraphAgent agent;
        [SerializeField, Min(0.5f)] private float lookDistance = 5f;
        [Header("Event Channels")]
        [SerializeField] private EventChannel dialogueEventChannel;
        [SerializeField] private EventChannel choiceEventChannel;
        [SerializeField] private EventChannel battleResultChannel;
        [Header("Presentation")]
        [SerializeField] private NpcBattleAnimationController animationController;

        private BlackboardVariable<Transform> _playerTransformVariable;
        private BlackboardVariable<float> _lookDistanceVariable;
        private BlackboardVariable<bool> _isPlayerInDialogVariable;
        private BlackboardVariable<bool> _dialogueChoiceVariable;
        private BlackboardVariable<bool> _battleRequestedVariable;
        private BlackboardVariable<BattleOutcome> _battleOutcomeVariable;
        private BlackboardVariable<EventChannel> _dialogueEventChannelVariable;
        private BlackboardVariable<EventChannel> _choiceEventChannelVariable;
        private BlackboardVariable<EventChannel> _battleResultChannelVariable;
        private bool _variablesBound;
        private string _dialogId;

        private void Awake()
        {
            agent = GetComponent<BehaviorGraphAgent>();

            TryBindBlackboardVariables();
            ApplyStaticConfiguration();
        }

        private void Start()
        {
            TryBindBlackboardVariables();
            ApplyStaticConfiguration();
        }

        private void OnEnable()
        {
            TryBindBlackboardVariables();
        }

        private void OnValidate()
        {
            {
                agent = GetComponent<BehaviorGraphAgent>();
            }
        }

        public void AssignDialogueId(string dialogId)
        {
            _dialogId = dialogId;
        }

        public void ConfigurePlayer(Transform playerTransform)
        {
            AssignValue(_playerTransformVariable, BlackboardKeys.PlayerTransform, playerTransform);
        }

        private void ApplyStaticConfiguration()
        {
            AssignValue(_lookDistanceVariable, BlackboardKeys.LookDistance, lookDistance);
            AssignValue(_dialogueEventChannelVariable, BlackboardKeys.DialogueEventChannel, dialogueEventChannel);
            AssignValue(_choiceEventChannelVariable, BlackboardKeys.ChoiceEventChannel, choiceEventChannel);
            AssignValue(_battleResultChannelVariable, BlackboardKeys.BattleResultChannel, battleResultChannel);
        }

        public void NotifyDialogOpened()
        {
            AssignValue(_isPlayerInDialogVariable, BlackboardKeys.IsPlayerInDialog, true);
            ResolveDialogueChannel()?.SendEventMessage();
            animationController?.PlayAnimation(NpcAnimationType.Greeting);
        }

        public void NotifyDialogClosed()
        {
            AssignValue(_isPlayerInDialogVariable, BlackboardKeys.IsPlayerInDialog, false);
            AssignValue(_dialogueChoiceVariable, BlackboardKeys.DialogueChoiceOpened, false);
            AssignValue(_battleRequestedVariable, BlackboardKeys.BattleRequested, false);
            animationController?.PlayAnimation(NpcAnimationType.Bye);
        }

        public void NotifyChoiceWindowState(bool isOpen)
        {
            AssignValue(_dialogueChoiceVariable, BlackboardKeys.DialogueChoiceOpened, isOpen);
            if (isOpen)
            {
                ResolveChoiceChannel()?.SendEventMessage();
            }
        }

        public void NotifyBattleRequested()
        {
            AssignValue(_battleRequestedVariable, BlackboardKeys.BattleRequested, true);
            animationController?.PlayAnimation(NpcAnimationType.BattleStart);
        }

        public void NotifyBattleOutcome(BattleOutcome outcome)
        {
            if (outcome == BattleOutcome.Unknown)
            {
                return;
            }

            AssignValue(_battleOutcomeVariable, BlackboardKeys.BattleOutcome, outcome);
            ResolveBattleResultChannel()?.SendEventMessage();
            AssignValue(_battleRequestedVariable, BlackboardKeys.BattleRequested, false);

            if (animationController == null)
                return;

            var animation = outcome == BattleOutcome.Victory
                ? NpcAnimationType.BattleLost
                : NpcAnimationType.BattleWon;
            animationController.PlayAnimation(animation);
        }

        private void TryBindBlackboardVariables()
        {
            if (_variablesBound || agent == null || agent.Graph == null)
            {
                return;
            }

            BindVariable(BlackboardKeys.PlayerTransform, ref _playerTransformVariable);
            BindVariable(BlackboardKeys.LookDistance, ref _lookDistanceVariable);
            BindVariable(BlackboardKeys.IsPlayerInDialog, ref _isPlayerInDialogVariable);
            BindVariable(BlackboardKeys.DialogueChoiceOpened, ref _dialogueChoiceVariable);
            BindVariable(BlackboardKeys.BattleRequested, ref _battleRequestedVariable);
            BindVariable(BlackboardKeys.BattleOutcome, ref _battleOutcomeVariable);
            BindVariable(BlackboardKeys.DialogueEventChannel, ref _dialogueEventChannelVariable);
            BindVariable(BlackboardKeys.ChoiceEventChannel, ref _choiceEventChannelVariable);
            BindVariable(BlackboardKeys.BattleResultChannel, ref _battleResultChannelVariable);

            _variablesBound = true;
        }

        private void BindVariable<T>(string key, ref BlackboardVariable<T> storage)
        {
            if (agent == null || string.IsNullOrEmpty(key) || storage != null)
                return;

            if (!agent.GetVariable(key, out storage))
            {
                Debug.LogWarning(
                    $"[NpcBehaviorGraphBridge] Blackboard variable '{key}' is not defined for dialog '{_dialogId}'",
                    this);
            }
        }

        private void AssignValue<T>(BlackboardVariable<T> variable, string variableKey, T value)
        {
            if (variable != null)
            {
                variable.Value = value;
            }
            else
            {
                TrySetBlackboardValue(variableKey, value);
            }
        }

        private EventChannel ResolveDialogueChannel()
        {
            return _dialogueEventChannelVariable?.Value ?? dialogueEventChannel;
        }

        private EventChannel ResolveChoiceChannel()
        {
            return _choiceEventChannelVariable?.Value ?? choiceEventChannel;
        }

        private EventChannel ResolveBattleResultChannel()
        {
            return _battleResultChannelVariable?.Value ?? battleResultChannel;
        }

        private void TrySetBlackboardValue<T>(string variable, T value)
        {
            if (agent == null || string.IsNullOrEmpty(variable))
            {
                return;
            }

            if (!agent.SetVariableValue(variable, value))
            {
                Debug.LogWarning(
                    $"[NpcBehaviorGraphBridge] Failed to set blackboard variable '{variable}' on dialog '{_dialogId}'",
                    this);
            }
        }
    }
}
