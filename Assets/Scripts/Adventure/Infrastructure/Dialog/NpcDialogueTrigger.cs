using Adventure.Application.Dialog;
using Adventure.Domain.Dialog;
using Adventure.Domain.Progression;
using Adventure.Infrastructure.Interaction;
using Adventure.Integration.Battle;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using static Adventure.Infrastructure.Dialog.NpcAnimationController;

namespace Adventure.Infrastructure.Dialog
{
    public interface IInitialBattleReturnNpc
    {
        bool TryStartReturnDialog(ArmyLineupSO battleLineup, BattleOutcome outcome);
    }

    [RequireComponent(typeof(NpcBehaviorGraphBridge))]
    public sealed class NpcDialogueTrigger : MonoBehaviour, IInteractable, IInitialBattleReturnNpc
    {
        [SerializeField] private DialogueGraphSO dialogue;
        [SerializeField] ArmyLineupSO lineup;
        [SerializeField] private ArmyLineupSO victoryReward;
        [SerializeField] string Name;
        [SerializeField] private string returnNpcKey;
        [SerializeField] private List<StoryFlagDefinitionSO> requiredFlags = new();
        [SerializeField] private DialogueGraphSO fallbackDialogue;
        [SerializeField] private NpcAnimationController battleAnimationController;
        [SerializeField] private NpcBehaviorGraphBridge behaviorGraphBridge;
        private DialogVM _dialogVM;
        private NpcBehaviorGraphRegistry _behaviorGraphRegistry;
        private IStoryFlagsService _storyFlagsService;
        public bool CanInteract => ResolveDialogueToStart() != null;
        public string ReturnNpcKey => ResolveReturnNpcKey();

        private void Reset()
        {
            ResolveBehaviorBridge();
        }

        private void OnValidate()
        {
            ResolveBehaviorBridge();
        }

        private void Awake()
        {
            NpcRendererShadowSettings.Apply(gameObject);
        }

        [Inject]
        public void Construct(
            DialogVM dialogVM,
            NpcBehaviorGraphRegistry behaviorGraphRegistry,
            IStoryFlagsService storyFlagsService)
        {
            _dialogVM = dialogVM;
            _behaviorGraphRegistry = behaviorGraphRegistry;
            _storyFlagsService = storyFlagsService;
            ResolveBehaviorBridge();
            EnsureDialogIDExist();
            RegisterBridge();
        }

        private void OnEnable()
        {
            NpcRendererShadowSettings.Apply(gameObject);
            RegisterBridge();
        }

        private void OnDisable()
        {
            UnregisterBridge();
        }
        private void EnsureDialogIDExist()
        {
            if (_dialogVM == null)
                return;

            EnsureDialogExists(dialogue);
            EnsureDialogExists(fallbackDialogue);
        }

        public void Interact(PlayerInteractionContext context)
        {
            var dialogueToStart = ResolveDialogueToStart();
            if (dialogueToStart == null)
            {
                Debug.LogWarning($"Dialogue asset is not available or still locked for {name}", this);
                return;
            }

            _behaviorGraphRegistry?.ConfigureDialogPlayer(dialogueToStart.Id, context.PlayerTransform, ReturnNpcKey);
            _behaviorGraphRegistry?.Activate(dialogueToStart.Id, behaviorGraphBridge);
            battleAnimationController?.PlayAnimation(NpcAnimationType.Greeting);
            bool isDialogStarted = _dialogVM.TryStartDialog(
                dialogueToStart.Id,
                lineup,
                dialogueToStart.StartNodeId,
                victoryReward: victoryReward,
                victoryRewardId: ResolveVictoryRewardId(dialogueToStart),
                returnNpcKey: ReturnNpcKey);
            if (!isDialogStarted)
            {
                _behaviorGraphRegistry?.ClearPending(dialogueToStart.Id);
                _behaviorGraphRegistry?.ClearActive(dialogueToStart.Id, behaviorGraphBridge);
                Debug.LogWarning($"Dialogue '{dialogueToStart.Id}' could not be started");
                return;
            }
        }

        public bool TryStartReturnDialog(ArmyLineupSO battleLineup, BattleOutcome outcome)
        {
            var dialogueToStart = ResolveDialogueToStart();
            if (dialogueToStart == null)
            {
                Debug.LogWarning($"Return dialogue asset is not available or still locked for {name}", this);
                return false;
            }

            if (_dialogVM == null)
            {
                Debug.LogWarning($"DialogVM is not injected for return NPC {name}", this);
                return false;
            }

            ResolveBehaviorBridge();
            _behaviorGraphRegistry?.Activate(dialogueToStart.Id, behaviorGraphBridge);

            var startNodeId = ResolveReturnStartNodeId(dialogueToStart, outcome);
            var isDialogStarted = _dialogVM.TryStartDialog(
                dialogueToStart.Id,
                battleLineup != null ? battleLineup : lineup,
                startNodeId,
                victoryReward,
                ResolveVictoryRewardId(dialogueToStart),
                ReturnNpcKey);

            if (!isDialogStarted)
            {
                _behaviorGraphRegistry?.ClearActive(dialogueToStart.Id, behaviorGraphBridge);
                Debug.LogWarning($"Return dialogue '{dialogueToStart.Id}' could not be started");
                return false;
            }

            if (outcome != BattleOutcome.Unknown)
            {
                _behaviorGraphRegistry?.NotifyBattleOutcome(outcome);
            }

            return true;
        }

        private static string ResolveReturnStartNodeId(DialogueGraphSO dialogueToStart, BattleOutcome outcome)
        {
            if (dialogueToStart == null)
            {
                return null;
            }

            var fallbackNodeId = string.IsNullOrEmpty(dialogueToStart.StartNodeId)
                ? null
                : dialogueToStart.StartNodeId;

            if (outcome == BattleOutcome.Unknown)
            {
                return fallbackNodeId;
            }

            var graph = dialogueToStart.ToDomain();
            foreach (var node in graph.Nodes)
            {
                var choices = node?.Choices;
                if (choices == null)
                {
                    continue;
                }

                for (var i = 0; i < choices.Count; i++)
                {
                    var choice = choices[i];
                    if (choice == null || !IsBattleChoice(choice.Action))
                    {
                        continue;
                    }

                    var resultNodeId = outcome == BattleOutcome.PlayerWon
                        ? choice.BattleVictoryNodeId
                        : choice.BattleDefeatNodeId;

                    if (!string.IsNullOrEmpty(resultNodeId))
                    {
                        return resultNodeId;
                    }

                    if (!string.IsNullOrEmpty(choice.NextNodeId))
                    {
                        return choice.NextNodeId;
                    }
                }
            }

            return fallbackNodeId;
        }

        private static bool IsBattleChoice(DialogueChoiceAction action)
        {
            return action == DialogueChoiceAction.StartBattle
                || action == DialogueChoiceAction.StartOnlineBattle;
        }

        public bool TryStopCurrentDialogueFromNpc()
        {
            if (dialogue == null && fallbackDialogue == null)
            {
                Debug.LogWarning($"Dialogue asset is not set for {name}", this);
                return false;
            }

            if (_dialogVM == null)
            {
                Debug.LogWarning($"DialogVM is not injected for {name}", this);
                return false;
            }

            if (!MatchesActiveDialogue(_dialogVM.ActiveDialogId))
            {
                return false;
            }

            ResolveBehaviorBridge();
            _behaviorGraphRegistry?.Activate(_dialogVM.ActiveDialogId, behaviorGraphBridge);
            battleAnimationController?.PlayAnimation(NpcAnimationType.Bye);
            _dialogVM.Close();
            return true;
        }

        public void SetLineup(ArmyLineupSO newLineup)
        {
            lineup = newLineup;
        }

        public string GetPrompt() => "Talk with " + Name;

        private void RegisterBridge()
        {
            ResolveBehaviorBridge();

            if ((dialogue == null && fallbackDialogue == null) || behaviorGraphBridge == null || _behaviorGraphRegistry == null)
            {
                return;
            }

            RegisterDialogue(dialogue);
            RegisterDialogue(fallbackDialogue);
        }

        private void UnregisterBridge()
        {
            ResolveBehaviorBridge();

            if ((dialogue == null && fallbackDialogue == null) || behaviorGraphBridge == null || _behaviorGraphRegistry == null)
            {
                return;
            }

            UnregisterDialogue(dialogue);
            UnregisterDialogue(fallbackDialogue);
        }

        private void ResolveBehaviorBridge()
        {
            if (behaviorGraphBridge != null)
            {
                return;
            }

            behaviorGraphBridge = GetComponent<NpcBehaviorGraphBridge>();
        }

        private DialogueGraphSO ResolveDialogueToStart()
        {
            if (dialogue == null && fallbackDialogue == null)
            {
                return null;
            }

            if (HasAllRequiredFlags())
            {
                return dialogue;
            }

            return fallbackDialogue;
        }

        private bool HasAllRequiredFlags()
        {
            if (requiredFlags == null || requiredFlags.Count == 0)
            {
                return true;
            }

            foreach (var flag in requiredFlags)
            {
                if (flag == null || string.IsNullOrWhiteSpace(flag.Id))
                {
                    continue;
                }

                if (!(_storyFlagsService?.Has(flag.Id) ?? false))
                {
                    return false;
                }
            }

            return true;
        }

        private bool MatchesActiveDialogue(string dialogId)
        {
            if (string.IsNullOrWhiteSpace(dialogId))
            {
                return false;
            }

            return (dialogue != null && dialogue.Id == dialogId)
                || (fallbackDialogue != null && fallbackDialogue.Id == dialogId);
        }

        private string ResolveVictoryRewardId(DialogueGraphSO dialogueAsset)
        {
            if (dialogueAsset == null || string.IsNullOrWhiteSpace(dialogueAsset.Id))
            {
                return null;
            }

            var npcKey = ReturnNpcKey;
            return string.IsNullOrWhiteSpace(npcKey)
                ? $"npc-dialogue:{dialogueAsset.Id}:victory-reward"
                : $"npc-dialogue:{dialogueAsset.Id}:{npcKey}:victory-reward";
        }

        private void EnsureDialogExists(DialogueGraphSO dialogueAsset)
        {
            if (dialogueAsset == null)
            {
                return;
            }

            if (!_dialogVM.DialogExist(dialogueAsset.Id))
            {
                Debug.LogWarning("Dialog wih id " + dialogueAsset.Id + " doesn not exist on go " + name);
            }
        }

        private void RegisterDialogue(DialogueGraphSO dialogueAsset)
        {
            if (dialogueAsset == null)
            {
                return;
            }

            _behaviorGraphRegistry.Register(dialogueAsset.Id, ReturnNpcKey, behaviorGraphBridge);
        }

        private void UnregisterDialogue(DialogueGraphSO dialogueAsset)
        {
            if (dialogueAsset == null)
            {
                return;
            }

            _behaviorGraphRegistry.Unregister(dialogueAsset.Id, ReturnNpcKey, behaviorGraphBridge);
        }

        private string ResolveReturnNpcKey()
        {
            if (!string.IsNullOrWhiteSpace(returnNpcKey))
            {
                return returnNpcKey.Trim();
            }

            return BuildHierarchyKey(transform);
        }

        private static string BuildHierarchyKey(Transform source)
        {
            if (source == null)
            {
                return null;
            }

            var parts = new List<string>();
            var current = source;
            while (current != null)
            {
                parts.Add($"{current.name}[{current.GetSiblingIndex()}]");
                current = current.parent;
            }

            parts.Reverse();

            var scene = source.gameObject.scene;
            var sceneKey = !string.IsNullOrWhiteSpace(scene.path)
                ? scene.path
                : scene.name;
            return $"{sceneKey}:{string.Join("/", parts)}";
        }
    }
}
