using Adventure.Application.Dialog;
using Adventure.Infrastructure.Interaction;
using Adventure.Integration.Battle;
using UnityEngine;
using Zenject;

namespace Adventure.Infrastructure.Dialog
{
    public sealed class NpcDialogueTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] private string dialogueId;
        [SerializeField] ArmyLineupSO lineup;
        private DialogVM _dialogVM;
        [Inject]
        public void Construct(DialogVM dialogVM)
        {
            _dialogVM = dialogVM;
        }
        public void Interact(PlayerInteractionContext context)
        {
            bool isDialogStarted = _dialogVM.TryStartDialog(dialogueId, lineup);
            if (!isDialogStarted)
            {
                Debug.LogWarning($"Dialogue '{dialogueId}' could not be started");
            }
        }

        public string GetPrompt() => "Talk";
    }
}
