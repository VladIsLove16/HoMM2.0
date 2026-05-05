using System.Collections.Generic;
using Unity.Behavior;

namespace Adventure.Domain.Dialog
{
    public sealed class DialogueChoice
    {
        public DialogueChoice(
            string id,
            string text,
            string nextNodeId,
            DialogueChoiceAction action,
            string battleVictoryNodeId = null,
            string battleDefeatNodeId = null,
            IReadOnlyList<string> requiredFlagIds = null,
            IReadOnlyList<string> grantedFlagIds = null,
            bool hideIfLocked = false,
            string lockedText = null,
            bool isAvailable = true)
        {
            Id = id;
            Text = text;
            NextNodeId = nextNodeId;
            Action = action;
            BattleVictoryNodeId = string.IsNullOrEmpty(battleVictoryNodeId) ? nextNodeId : battleVictoryNodeId;
            BattleDefeatNodeId = string.IsNullOrEmpty(battleDefeatNodeId) ? BattleVictoryNodeId : battleDefeatNodeId;
            RequiredFlagIds = requiredFlagIds ?? System.Array.Empty<string>();
            GrantedFlagIds = grantedFlagIds ?? System.Array.Empty<string>();
            HideIfLocked = hideIfLocked;
            LockedText = lockedText;
            IsAvailable = isAvailable;
        }

        public string Id { get; }
        public string Text { get; }
        public string NextNodeId { get; }
        public DialogueChoiceAction Action { get; }
        public string BattleVictoryNodeId { get; }
        public string BattleDefeatNodeId { get; }
        public IReadOnlyList<string> RequiredFlagIds { get; }
        public IReadOnlyList<string> GrantedFlagIds { get; }
        public bool HideIfLocked { get; }
        public string LockedText { get; }
        public bool IsAvailable { get; }

        public DialogueChoice WithPresentation(string text, bool isAvailable)
        {
            return new DialogueChoice(
                Id,
                text,
                NextNodeId,
                Action,
                BattleVictoryNodeId,
                BattleDefeatNodeId,
                RequiredFlagIds,
                GrantedFlagIds,
                HideIfLocked,
                LockedText,
                isAvailable);
        }
    }
    [BlackboardEnum]    
    public enum DialogueChoiceAction
    {
        None,
        StartBattle,
        EndDialogue,
        StartOnlineBattle
    }
}
