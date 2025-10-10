using Adventure.Infrastructure.Interaction;
using UnityEngine;

namespace Adventure.Infrastructure.Dialog
{
    public sealed class NpcDialogueTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] private string dialogueId;
        [SerializeField] private AdventureDialogueOrchestrator orchestrator;

        public void Interact(PlayerInteractionContext context)
        {
            if (orchestrator == null)
            {
                Debug.LogWarning("Dialogue orchestrator not set");
                return;
            }

            if (!orchestrator.StartDialog(dialogueId))
            {
                Debug.LogWarning($"Dialogue '{dialogueId}' could not be started");
            }
        }
    }
}
