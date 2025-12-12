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
        }

        [SerializeField] private BehaviorGraphAgent agent;
        [SerializeField, Min(0.5f)] private float lookDistance = 5f;
        [Header("Event Channels")]
        [SerializeField] private DialogueEventChannel dialogueEventChannel;
        [SerializeField] private ChoiceEventChannel choiceEventChannel;
        [SerializeField] private BattleResultChannel battleResultChannel;
        [Header("Presentation")]
        [SerializeField] private NpcBattleAnimationController animationController;

        private Transform _playerTransform;
        private string _dialogId;

        private void Awake()
        {
            if (agent == null)
            {
                agent = GetComponent<BehaviorGraphAgent>();
            }

            ApplyStaticConfiguration();
        }

        private void OnValidate()
        {
            if (agent == null)
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
            _playerTransform = playerTransform;
            TrySetBlackboardValue(BlackboardKeys.PlayerTransform, _playerTransform);
        }

        private void ApplyStaticConfiguration()
        {
            TrySetBlackboardValue(BlackboardKeys.LookDistance, lookDistance);
            TrySetBlackboardValue(BlackboardKeys.DialogueEventChannel, dialogueEventChannel);
            TrySetBlackboardValue(BlackboardKeys.ChoiceEventChannel, choiceEventChannel);
        }

        public void NotifyDialogOpened()
        {
            TrySetBlackboardValue(BlackboardKeys.IsPlayerInDialog, true);
            dialogueEventChannel?.SendEventMessage();
            animationController?.PlayAnimation(NpcAnimationType.Greeting);
        }

        public void NotifyDialogClosed()
        {
            TrySetBlackboardValue(BlackboardKeys.IsPlayerInDialog, false);
            TrySetBlackboardValue(BlackboardKeys.DialogueChoiceOpened, false);
            TrySetBlackboardValue(BlackboardKeys.BattleRequested, false);
            animationController?.PlayAnimation(NpcAnimationType.Bye);
        }

        public void NotifyChoiceWindowState(bool isOpen)
        {
            TrySetBlackboardValue(BlackboardKeys.DialogueChoiceOpened, isOpen);
            if (isOpen)
            {
                choiceEventChannel?.SendEventMessage();
            }
        }

        public void NotifyBattleRequested()
        {
            TrySetBlackboardValue(BlackboardKeys.BattleRequested, true);
            animationController?.PlayAnimation(NpcAnimationType.BattleStart);
        }

        public void NotifyBattleOutcome(BattleOutcome outcome)
        {
            if (outcome == BattleOutcome.Unknown)
            {
                return;
            }

            TrySetBlackboardValue(BlackboardKeys.BattleOutcome, outcome);
            battleResultChannel?.SendEventMessage();
            TrySetBlackboardValue(BlackboardKeys.BattleRequested, false);

            if (animationController == null)
                return;

            var animation = outcome == BattleOutcome.Victory
                ? NpcAnimationType.BattleLost
                : NpcAnimationType.BattleWon;
            animationController.PlayAnimation(animation);
        }

        private void TrySetBlackboardValue<T>(string variable, T value)
        {
            if (agent == null || string.IsNullOrEmpty(variable))
            {
                return;
            }

            if (!agent.SetVariableValue(variable, value) && Application.isPlaying)
            {
                Debug.LogWarning(
                    $"[NpcBehaviorGraphBridge] Failed to set blackboard variable '{variable}' on dialog '{_dialogId}'",
                    this);
            }
        }
    }
}
