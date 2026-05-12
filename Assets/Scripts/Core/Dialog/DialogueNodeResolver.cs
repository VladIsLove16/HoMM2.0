using System.Collections.Generic;

namespace Adventure.Domain.Dialog
{
    public sealed class DialogueNodeResolver
    {
        private readonly IDialogueTextResolver _textResolver;

        public DialogueNodeResolver(IDialogueTextResolver textResolver)
        {
            _textResolver = textResolver;
        }

        public DialogueNode Resolve(DialogueNode node)
        {
            if (node == null)
                return null;

            var resolvedSpeaker = _textResolver?.Resolve(node.Speaker) ?? node.Speaker;
            var resolvedText = _textResolver?.Resolve(node.Text) ?? node.Text;

            var choices = node.Choices ?? new List<DialogueChoice>();
            var resolvedChoices = new List<DialogueChoice>(choices.Count);
            foreach (var choice in choices)
            {
                var resolvedChoiceText = _textResolver?.Resolve(choice.Text) ?? choice.Text;
                resolvedChoices.Add(new DialogueChoice(
                    choice.Id,
                    resolvedChoiceText,
                    choice.NextNodeId,
                    choice.Action,
                    choice.BattleVictoryNodeId,
                    choice.BattleDefeatNodeId,
                    choice.RequiredFlagIds,
                    choice.GrantedFlagIds,
                    choice.HideIfLocked,
                    choice.LockedText,
                    choice.IsAvailable));
            }

            return new DialogueNode(node.Id, resolvedSpeaker, resolvedText, resolvedChoices);
        }
    }
}
