using System.Collections.Generic;
using Adventure.Domain.Dialog;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Adventure/Dialogue/Node", fileName = "DialogueNode")]
public sealed class DialogueNodeSO : ScriptableObject
{
    [System.Serializable]
    private struct Choice
    {
        public string Id;
        [TextArea] public string Text;
        public LocalizedString TextLocalized;
        public DialogueChoiceAction Action;
        public DialogueNodeSO NextNode;
        public DialogueNodeSO VictoryNode;
        public DialogueNodeSO DefeatNode;
    }

    [SerializeField] private string nodeId;
    [SerializeField] private string speaker;
    [SerializeField] private LocalizedString speakerLocalized;
    [TextArea] [SerializeField] private string text;
    [SerializeField] private LocalizedString textLocalized;
    [SerializeField] private List<Choice> choices = new List<Choice>();
    [SerializeField] private KeyWordsParser keyWordsParser;
    public string NodeId => nodeId;

    public DialogueNode ToDomain()
    {
        var resolvedSpeaker = ResolveLocalizedString(speakerLocalized, speaker);
        var resolvedText = ResolveLocalizedString(textLocalized, text);
        var parsedText = keyWordsParser.Parse(resolvedText);
        var domainChoices = new List<DialogueChoice>(choices.Count);
        foreach (var choice in choices)
        {
            var nextId = choice.NextNode != null ? choice.NextNode.nodeId : null;
            var victoryId = choice.VictoryNode != null ? choice.VictoryNode.NodeId : nextId;
            var defeatId = choice.DefeatNode != null ? choice.DefeatNode.NodeId : victoryId;
            var resolvedChoiceText = ResolveLocalizedString(choice.TextLocalized, choice.Text);
            domainChoices.Add(new DialogueChoice(choice.Id, resolvedChoiceText, nextId, choice.Action, victoryId, defeatId));
        }
        return new DialogueNode(nodeId, resolvedSpeaker, resolvedText, domainChoices);
    }

    private static string ResolveLocalizedString(LocalizedString entry, string fallback)
    {
        if (entry != null && !entry.IsEmpty)
        {
            var value = entry.GetLocalizedString();
            if (!string.IsNullOrEmpty(value))
                return value;
        }

        return fallback ?? string.Empty;
    }
}
