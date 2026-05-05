using Adventure.Application.Dialog;
using Adventure.Domain.Progression;
using Adventure.Infrastructure.Interaction;
using Adventure.Integration.Battle;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using static Adventure.Infrastructure.Dialog.NpcAnimationController;

namespace Adventure.Infrastructure.Dialog
{
    [RequireComponent(typeof(NpcBehaviorGraphBridge))]
    public sealed class NpcDialogueTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueGraphSO dialogue;
        [SerializeField] ArmyLineupSO lineup;
        [SerializeField] string Name;
        [SerializeField] private List<StoryFlagDefinitionSO> requiredFlags = new();
        [SerializeField] private DialogueGraphSO fallbackDialogue;
        [SerializeField] private NpcAnimationController battleAnimationController;
        [SerializeField] private NpcBehaviorGraphBridge behaviorGraphBridge;
        private DialogVM _dialogVM;
        private NpcBehaviorGraphRegistry _behaviorGraphRegistry;
        private IStoryFlagsService _storyFlagsService;

        private void Reset()
        {
            ResolveBehaviorBridge();
        }

        private void OnValidate()
        {
            ResolveBehaviorBridge();
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

            _behaviorGraphRegistry?.ConfigureDialogPlayer(dialogueToStart.Id, context.PlayerTransform);
            _behaviorGraphRegistry?.Activate(dialogueToStart.Id, behaviorGraphBridge);
            battleAnimationController?.PlayAnimation(NpcAnimationType.Greeting);
            bool isDialogStarted = _dialogVM.TryStartDialog(dialogueToStart.Id, lineup);
            if (!isDialogStarted)
            {
                _behaviorGraphRegistry?.ClearPending(dialogueToStart.Id);
                _behaviorGraphRegistry?.ClearActive(dialogueToStart.Id, behaviorGraphBridge);
                Debug.LogWarning($"Dialogue '{dialogueToStart.Id}' could not be started");
                return;
            }
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

            _behaviorGraphRegistry.Register(dialogueAsset.Id, behaviorGraphBridge);
        }

        private void UnregisterDialogue(DialogueGraphSO dialogueAsset)
        {
            if (dialogueAsset == null)
            {
                return;
            }

            _behaviorGraphRegistry.Unregister(dialogueAsset.Id, behaviorGraphBridge);
        }
    }
}
