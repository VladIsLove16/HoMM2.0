using Adventure.Domain.Dialog;
using Adventure.Infrastructure.Movement;
using Adventure.Infrastructure.State;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Serialization;
using static Adventure.Infrastructure.Dialog.NpcAnimationController;

namespace Adventure.Infrastructure.Dialog
{
    [DisallowMultipleComponent]
    public sealed class NpcBehaviorGraphBridge : MonoBehaviour
    {
        private const string DebugPrefix = "[NpcBehaviorGraphBridge]";
        private static readonly bool VerboseLogging = false;

        private static class BlackboardKeys
        {
            public const string PlayerMovementController = "Player Movement Controller";
            public const string TalkingDistance = "TalkingDistance";
            public const string ReactedToReachingTalkingDistance = "Reacted to reaching talking distance";
            public const string TimesPlayerClosedDialog = "Times player closed dialog";
            public const string State = "State";
            public const string BattleOutcome = "Last BattleOutcome";
            public const string LastChosenDialogOption = "Last chosen dialog option";
            public const string DialogueEventChannel = "New Player  talking with npc";
            public const string ChoiceEventChannel = "New Player chosen dialog option";
            public const string BattleResultChannel = "BattleFinishedChannel";
        }

        [SerializeField] private BehaviorGraphAgent agent;
        [SerializeField, Min(0.5f)] private float lookDistance = 5f;
        [Header("Event Channels")]
        [Tooltip("Template asset. A dedicated runtime instance is cloned per NPC bridge.")]
        [FormerlySerializedAs("dialogueEventChannelTemplate")]
        [FormerlySerializedAs("dialogueEventChannel")]
        [SerializeField] private PlayerTalkingWithNpcStateChannel dialogStateChannelTemplate;
        [Tooltip("Template asset. A dedicated runtime instance is cloned per NPC bridge.")]
        [FormerlySerializedAs("choiceEventChannel")]
        [SerializeField] private PlayerChosenDialogOption choiceEventChannelTemplate;
        [Tooltip("Template asset. A dedicated runtime instance is cloned per NPC bridge.")]
        [FormerlySerializedAs("battleResultChannel")]
        [SerializeField] private BattleFinishedChannel battleResultChannelTemplate;
        [Header("Presentation")]
        [SerializeField] private NpcAnimationController animationController;

        private BlackboardVariable<PlayerMovementController> _playerMovementControllerVariable;
        private BlackboardVariable<float> _talkingDistanceVariable;
        private BlackboardVariable<bool> _reactedToReachingTalkingDistanceVariable;
        private BlackboardVariable<int> _timesPlayerClosedDialogVariable;
        private BlackboardVariable<NPCState> _stateVariable;
        private BlackboardVariable<BattleOutcome> _battleOutcomeVariable;
        private BlackboardVariable<DialogueChoiceAction> _lastChosenDialogOptionVariable;
        private BlackboardVariable<PlayerTalkingWithNpcStateChannel> _dialogStateChannelVariable;
        private BlackboardVariable<PlayerChosenDialogOption> _choiceEventChannelVariable;
        private BlackboardVariable<BattleFinishedChannel> _battleResultChannelVariable;
        private bool _variablesBound;
        private string _dialogId;
        private PlayerTalkingWithNpcStateChannel _runtimeDialogStateChannel;
        private PlayerChosenDialogOption _runtimeChoiceEventChannel;
        private BattleFinishedChannel _runtimeBattleResultChannel;
        private PlayerMovementController _configuredPlayerMovementController;
        private bool _hasConfiguredPlayerMovementController;

        private void Awake()
        {
            agent = GetComponent<BehaviorGraphAgent>();

            CreateRuntimeEventChannels();
            TryBindBlackboardVariables();
            ApplyStaticConfiguration();
            ApplyPlayerConfiguration();
        }

        private void Start()
        {
            CreateRuntimeEventChannels();
            TryBindBlackboardVariables();
            ApplyStaticConfiguration();
            ApplyPlayerConfiguration();
        }

        private void OnEnable()
        {
            CreateRuntimeEventChannels();
            TryBindBlackboardVariables();
            ApplyStaticConfiguration();
            ApplyPlayerConfiguration();
        }

        private void OnDestroy()
        {
            DestroyRuntimeEventChannel(_runtimeDialogStateChannel);
            DestroyRuntimeEventChannel(_runtimeChoiceEventChannel);
            DestroyRuntimeEventChannel(_runtimeBattleResultChannel);
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

        public void ConfigurePlayer(PlayerMovementController playerMovementController)
        {
            _configuredPlayerMovementController = playerMovementController;
            _hasConfiguredPlayerMovementController = true;

            TryBindBlackboardVariables();
            ApplyPlayerConfiguration();
        }

        private void ApplyStaticConfiguration()
        {
            AssignValue(_talkingDistanceVariable, BlackboardKeys.TalkingDistance, lookDistance, false);
            AssignValue(_dialogStateChannelVariable, BlackboardKeys.DialogueEventChannel, _runtimeDialogStateChannel ?? dialogStateChannelTemplate, false);
            AssignValue(_choiceEventChannelVariable, BlackboardKeys.ChoiceEventChannel, _runtimeChoiceEventChannel ?? choiceEventChannelTemplate, false);
            AssignValue(_battleResultChannelVariable, BlackboardKeys.BattleResultChannel, _runtimeBattleResultChannel ?? battleResultChannelTemplate, false);
        }

        private void ApplyPlayerConfiguration()
        {
            if (!_hasConfiguredPlayerMovementController)
            {
                return;
            }

            AssignValue(
                _playerMovementControllerVariable,
                BlackboardKeys.PlayerMovementController,
                _configuredPlayerMovementController,
                false);
        }

        public void NotifyDialogOpened()
        {
            ResolveDialogStateChannel()?.SendEventMessage();
            animationController?.PlayAnimation(NpcAnimationType.Greeting);
        }

        public void NotifyDialogClosed()
        {
            if (VerboseLogging)
            {
                Debug.Log(
                    $"{DebugPrefix} NotifyDialogClosed start npc='{name}' dialog='{_dialogId}' " +
                    $"reactedBound={_reactedToReachingTalkingDistanceVariable != null} " +
                    $"stateBound={_stateVariable != null} " +
                    $"timesClosedBound={_timesPlayerClosedDialogVariable != null}",
                    this);
            }

            var reactedAssigned = AssignValue(
                _reactedToReachingTalkingDistanceVariable,
                BlackboardKeys.ReactedToReachingTalkingDistance,
                true);
            var timesClosedAssigned = IncrementValue(
                _timesPlayerClosedDialogVariable,
                BlackboardKeys.TimesPlayerClosedDialog);
            var stateAssigned = AssignValue(_stateVariable, BlackboardKeys.State, NPCState.None);

            ResolveDialogStateChannel()?.SendEventMessage();
            animationController?.PlayAnimation(NpcAnimationType.Bye);

            if (VerboseLogging)
            {
                Debug.Log(
                    $"{DebugPrefix} NotifyDialogClosed end npc='{name}' dialog='{_dialogId}' " +
                    $"reactedAssigned={reactedAssigned} reactedValue={FormatValue(_reactedToReachingTalkingDistanceVariable)} " +
                    $"stateAssigned={stateAssigned} stateValue={FormatValue(_stateVariable)} " +
                    $"timesClosedAssigned={timesClosedAssigned} timesClosedValue={FormatValue(_timesPlayerClosedDialogVariable)}",
                    this);
            }
        }

        public void NotifyChoiceWindowState(bool isOpen)
        {
            _ = isOpen;
        }

        public void NotifyChoiceAction(DialogueChoiceAction action)
        {
            AssignValue(_lastChosenDialogOptionVariable, BlackboardKeys.LastChosenDialogOption, action);
            ResolveChoiceChannel()?.SendEventMessage(action);
        }

        public void NotifyBattleRequested()
        {
            animationController?.PlayAnimation(NpcAnimationType.BattleStart);
        }

        public void NotifyBattleOutcome(BattleOutcome outcome)
        {
            if (outcome == BattleOutcome.Unknown)
            {
                return;
            }

            AssignValue(_battleOutcomeVariable, BlackboardKeys.BattleOutcome, outcome);
            ResolveBattleResultChannel()?.SendEventMessage(outcome);

            if (animationController == null)
                return;

            var animation = outcome == BattleOutcome.PlayerWon
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

            BindVariable(BlackboardKeys.PlayerMovementController, ref _playerMovementControllerVariable);
            BindVariable(BlackboardKeys.TalkingDistance, ref _talkingDistanceVariable);
            BindVariable(BlackboardKeys.ReactedToReachingTalkingDistance, ref _reactedToReachingTalkingDistanceVariable);
            BindVariable(BlackboardKeys.TimesPlayerClosedDialog, ref _timesPlayerClosedDialogVariable);
            BindVariable(BlackboardKeys.State, ref _stateVariable);
            BindVariable(BlackboardKeys.BattleOutcome, ref _battleOutcomeVariable);
            BindVariable(BlackboardKeys.LastChosenDialogOption, ref _lastChosenDialogOptionVariable);
            BindVariable(BlackboardKeys.DialogueEventChannel, ref _dialogStateChannelVariable);
            BindVariable(BlackboardKeys.ChoiceEventChannel, ref _choiceEventChannelVariable);
            BindVariable(BlackboardKeys.BattleResultChannel, ref _battleResultChannelVariable);

            _variablesBound = true;
            ApplyStaticConfiguration();
            ApplyPlayerConfiguration();
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

        private bool AssignValue<T>(BlackboardVariable<T> variable, string variableKey, T value, bool logIfMissing = true)
        {
            if (variable == null)
            {
                if (logIfMissing)
                {
                    Debug.LogWarning(
                        $"[NpcBehaviorGraphBridge] Blackboard variable '{variableKey}' is not bound for dialog '{_dialogId}'",
                        this);
                }

                return false;
            }

            if (agent != null && variable.GUID.Valid && agent.SetVariableValue(variable.GUID, value))
            {
                return true;
            }

            variable.Value = value;
            return true;
        }

        private bool IncrementValue(BlackboardVariable<int> variable, string variableKey)
        {
            if (variable == null)
            {
                Debug.LogWarning(
                    $"[NpcBehaviorGraphBridge] Blackboard variable '{variableKey}' is not bound for dialog '{_dialogId}'",
                    this);
                return false;
            }

            var nextValue = variable.Value + 1;
            return AssignValue(variable, variableKey, nextValue);
        }

        private PlayerTalkingWithNpcStateChannel ResolveDialogStateChannel()
        {
            return _runtimeDialogStateChannel ?? _dialogStateChannelVariable?.Value ?? dialogStateChannelTemplate;
        }

        private PlayerChosenDialogOption ResolveChoiceChannel()
        {
            return _runtimeChoiceEventChannel ?? _choiceEventChannelVariable?.Value ?? choiceEventChannelTemplate;
        }

        private BattleFinishedChannel ResolveBattleResultChannel()
        {
            return _runtimeBattleResultChannel ?? _battleResultChannelVariable?.Value ?? battleResultChannelTemplate;
        }

        private void CreateRuntimeEventChannels()
        {
            _runtimeDialogStateChannel ??= CreateRuntimeEventChannel(dialogStateChannelTemplate, "DialogueState");
            _runtimeChoiceEventChannel ??= CreateRuntimeEventChannel(choiceEventChannelTemplate, "Choice");
            _runtimeBattleResultChannel ??= CreateRuntimeEventChannel(battleResultChannelTemplate, "BattleResult");
        }

        private T CreateRuntimeEventChannel<T>(T template, string channelLabel) where T : ScriptableObject
        {
            if (template == null)
            {
                return null;
            }

            var runtimeInstance = Instantiate(template);
            var ownerName = string.IsNullOrWhiteSpace(name) ? "Npc" : name;
            runtimeInstance.name = $"{template.name} [{ownerName}:{channelLabel}:Runtime]";
            runtimeInstance.hideFlags = HideFlags.DontSave;
            return runtimeInstance;
        }

        private static void DestroyRuntimeEventChannel(Object channel)
        {
            if (channel == null)
            {
                return;
            }

            if (UnityEngine.Application.isPlaying)
            {
                Destroy(channel);
            }
            else
            {
                DestroyImmediate(channel);
            }
        }

        private static string FormatValue<T>(BlackboardVariable<T> variable)
        {
            return variable == null ? "null" : variable.Value?.ToString() ?? "null";
        }
    }
}
