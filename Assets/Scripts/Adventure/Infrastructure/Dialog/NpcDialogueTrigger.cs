using Adventure.Application.Dialog;
using Adventure.Infrastructure.Interaction;
using Adventure.Integration.Battle;
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
        [SerializeField] private NpcAnimationController battleAnimationController;
        [SerializeField] private NpcBehaviorGraphBridge behaviorGraphBridge;
        private DialogVM _dialogVM;
        private NpcBehaviorGraphRegistry _behaviorGraphRegistry;

        private void Reset()
        {
            ResolveBehaviorBridge();
        }

        private void OnValidate()
        {
            ResolveBehaviorBridge();
        }

        [Inject]
        public void Construct(DialogVM dialogVM, NpcBehaviorGraphRegistry behaviorGraphRegistry)
        {
            _dialogVM = dialogVM;
            _behaviorGraphRegistry = behaviorGraphRegistry;
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
            if (dialogue == null || _dialogVM == null)
                return;

            if (!_dialogVM.DialogExist(dialogue.Id))
                Debug.LogWarning("Dialog wih id " + dialogue.Id+ " doesn not exist on go " + name);
        }

        public void Interact(PlayerInteractionContext context)
        {
            if (dialogue == null)
            {
                Debug.LogWarning($"Dialogue asset is not set for {name}", this);
                return;
            }

            _behaviorGraphRegistry?.ConfigureDialogPlayer(dialogue.Id, context.PlayerTransform);
            _behaviorGraphRegistry?.Activate(dialogue.Id, behaviorGraphBridge);
            battleAnimationController?.PlayAnimation(NpcAnimationType.Greeting);
            bool isDialogStarted = _dialogVM.TryStartDialog(dialogue.Id, lineup);
            if (!isDialogStarted)
            {
                _behaviorGraphRegistry?.ClearPending(dialogue.Id);
                _behaviorGraphRegistry?.ClearActive(dialogue.Id, behaviorGraphBridge);
                Debug.LogWarning($"Dialogue '{dialogue.Id}' could not be started");
                return;
            }
        }

        public bool TryStopCurrentDialogueFromNpc()
        {
            if (dialogue == null)
            {
                Debug.LogWarning($"Dialogue asset is not set for {name}", this);
                return false;
            }

            if (_dialogVM == null)
            {
                Debug.LogWarning($"DialogVM is not injected for {name}", this);
                return false;
            }

            if (_dialogVM.ActiveDialogId != dialogue.Id)
            {
                return false;
            }

            ResolveBehaviorBridge();
            _behaviorGraphRegistry?.Activate(dialogue.Id, behaviorGraphBridge);
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

            if (dialogue == null || behaviorGraphBridge == null || _behaviorGraphRegistry == null)
            {
                return;
            }

            _behaviorGraphRegistry.Register(dialogue.Id, behaviorGraphBridge);
        }

        private void UnregisterBridge()
        {
            ResolveBehaviorBridge();

            if (dialogue == null || behaviorGraphBridge == null || _behaviorGraphRegistry == null)
            {
                return;
            }

            _behaviorGraphRegistry.Unregister(dialogue.Id, behaviorGraphBridge);
        }

        private void ResolveBehaviorBridge()
        {
            if (behaviorGraphBridge != null)
            {
                return;
            }

            behaviorGraphBridge = GetComponent<NpcBehaviorGraphBridge>();
        }
    }
}
