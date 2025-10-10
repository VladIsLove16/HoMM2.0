using Adventure.Infrastructure.Interaction;
using Adventure.Integration.Battle;
using UnityEngine;

namespace Adventure.Infrastructure.Dialog
{
    public sealed class NpcDialogueTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] private string dialogueId;
        [SerializeField] private AdventureDialogueOrchestrator orchestrator;
        [SerializeField] ArmyLineupSO lineup;

        public void Interact(PlayerInteractionContext context)
        {
            if (orchestrator == null)
            {
                Debug.LogWarning("Dialogue orchestrator not set");
                return;
            }

            bool isDialogStarted = orchestrator.StartDialog(dialogueId, lineup,);
            if (!isDialogStarted)
            {
                Debug.LogWarning($"Dialogue '{dialogueId}' could not be started");
            }
        }

        public string GetPrompt() => "Talk";
    }
}
