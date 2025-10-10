namespace Adventure.Domain.Dialog
{
    public sealed class DialogueChoice
    {
        public DialogueChoice(string id, string text, string nextNodeId, DialogueChoiceAction action)
        {
            Id = id;
            Text = text;
            NextNodeId = nextNodeId;
            Action = action;
        }

        public string Id { get; }
        public string Text { get; }
        public string NextNodeId { get; }
        public DialogueChoiceAction Action { get; }
    }

    public enum DialogueChoiceAction
    {
        None,
        StartBattle,
        EndDialogue
    }
}
