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
            string battleDefeatNodeId = null)
        {
            Id = id;
            Text = text;
            NextNodeId = nextNodeId;
            Action = action;
            BattleVictoryNodeId = string.IsNullOrEmpty(battleVictoryNodeId) ? nextNodeId : battleVictoryNodeId;
            BattleDefeatNodeId = string.IsNullOrEmpty(battleDefeatNodeId) ? BattleVictoryNodeId : battleDefeatNodeId;
        }

        public string Id { get; }
        public string Text { get; }
        public string NextNodeId { get; }
        public DialogueChoiceAction Action { get; }
        public string BattleVictoryNodeId { get; }
        public string BattleDefeatNodeId { get; }
    }

    public enum DialogueChoiceAction
    {
        None,
        StartBattle,
        EndDialogue
    }
}
