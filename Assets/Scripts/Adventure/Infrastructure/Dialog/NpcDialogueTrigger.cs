using Adventure.Application.Dialog;
using Adventure.Infrastructure.Interaction;
using Adventure.Integration.Battle;
using NaughtyAttributes;
using System;
using UnityEngine;
using Zenject;

namespace Adventure.Infrastructure.Dialog
{
    public sealed class NpcDialogueTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] private string dialogueId;
        [SerializeField] ArmyLineupSO lineup;
        [SerializeField] string Name;
        private DialogVM _dialogVM;
        [Inject]
        public void Construct(DialogVM dialogVM)
        {
            _dialogVM = dialogVM;
            EnsureDialogIDExist();
        }
        private void EnsureDialogIDExist()
        {
            if (!_dialogVM.DialogExist(dialogueId))
                Debug.LogWarning("Dialog wih id " + dialogueId + " doesn not exist on go " + name);
        }

        public void Interact(PlayerInteractionContext context)
        {
            bool isDialogStarted = _dialogVM.TryStartDialog(dialogueId, lineup);
            if (!isDialogStarted)
            {
                Debug.LogWarning($"Dialogue '{dialogueId}' could not be started");
            }
        }

        public string GetPrompt() => "Talk with " + Name;
    }
}
