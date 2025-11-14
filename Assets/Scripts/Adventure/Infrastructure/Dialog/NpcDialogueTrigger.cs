using Adventure.Application.Dialog;
using Adventure.Infrastructure.Interaction;
using Adventure.Integration.Battle;
using NaughtyAttributes;
using System;
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
        private DialogVM _dialogVM;
        [Inject]
        public void Construct(DialogVM dialogVM)
        {
            _dialogVM = dialogVM;
            EnsureDialogIDExist();
        }
        private void EnsureDialogIDExist()
        {
            if (!_dialogVM.DialogExist(dialogue.Id))
                Debug.LogWarning("Dialog wih id " + dialogue.Id+ " doesn not exist on go " + name);
        }

        public void Interact(PlayerInteractionContext context)
        {
            battleAnimationController?.PlayAnimation(NpcAnimationType.Greeting);
            bool isDialogStarted = _dialogVM.TryStartDialog(dialogue.Id, lineup);
            if (!isDialogStarted)
            {
                Debug.LogWarning($"Dialogue '{dialogue.Id}' could not be started");
            }
        }

        public void SetLineup(ArmyLineupSO newLineup)
        {
            lineup = newLineup;
        }

        public string GetPrompt() => "Talk with " + Name;
    }
}
