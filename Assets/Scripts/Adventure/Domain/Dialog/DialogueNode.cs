using System.Collections.Generic;

namespace Adventure.Domain.Dialog
{
    public sealed class DialogueNode
    {
        public DialogueNode(string id, string speaker, string text, IReadOnlyList<DialogueChoice> choices)
        {
            Id = id;
            Speaker = speaker;
            Text = text;
            Choices = choices;
        }

        public string Id { get; }
        public string Speaker { get; }
        public string Text { get; }
        public IReadOnlyList<DialogueChoice> Choices { get; }
    }
}
