using Adventure.Application.Dialog;
using Adventure.Infrastructure.Interaction;
using Adventure.Integration.Battle;
using UnityEngine;
using Zenject;
using static Adventure.Infrastructure.Dialog.NpcBattleAnimationController;

namespace Adventure.Infrastructure.Dialog
{
    public sealed class NpcDialogueTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueGraphSO dialogue;
        [SerializeField] ArmyLineupSO lineup;
        [SerializeField] string Name;
        [SerializeField] private NpcBattleAnimationController battleAnimationController;
        [SerializeField] private NpcBehaviorGraphBridge behaviorGraphBridge;
        private DialogVM _dialogVM;
        private NpcBehaviorGraphRegistry _behaviorGraphRegistry;
        [Inject]
        public void Construct(DialogVM dialogVM, NpcBehaviorGraphRegistry behaviorGraphRegistry)
        {
            _dialogVM = dialogVM;
            _behaviorGraphRegistry = behaviorGraphRegistry;
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

            battleAnimationController?.PlayAnimation(NpcAnimationType.Greeting);
            _behaviorGraphRegistry?.SetPendingDialog(dialogue.Id);
            bool isDialogStarted = _dialogVM.TryStartDialog(dialogue.Id, lineup);
            if (!isDialogStarted)
            {
                _behaviorGraphRegistry?.ClearPending(dialogue.Id);
                Debug.LogWarning($"Dialogue '{dialogue.Id}' could not be started");
                return;
            }

            _behaviorGraphRegistry?.ForceActivate(dialogue.Id);
        }

        public void SetLineup(ArmyLineupSO newLineup)
        {
            lineup = newLineup;
        }

        public string GetPrompt() => "Talk with " + Name;

        private void RegisterBridge()
        {
            if (dialogue == null || behaviorGraphBridge == null || _behaviorGraphRegistry == null)
            {
                return;
            }

            _behaviorGraphRegistry.Register(dialogue.Id, behaviorGraphBridge);
        }

        private void UnregisterBridge()
        {
            if (dialogue == null || behaviorGraphBridge == null || _behaviorGraphRegistry == null)
            {
                return;
            }

            _behaviorGraphRegistry.Unregister(dialogue.Id, behaviorGraphBridge);
        }
    }
}
