using System.Collections.Generic;
using Adventure.Domain.Dialog;
using UnityEngine;

[CreateAssetMenu(menuName = "Adventure/Dialogue/Node", fileName = "DialogueNode")]
public sealed class DialogueNodeSO : ScriptableObject
{
    [System.Serializable]
    private struct Choice
    {
        public string Id;
        [TextArea] public string Text;
        public DialogueChoiceAction Action;
        public DialogueNodeSO NextNode;
        public DialogueNodeSO VictoryNode;
        public DialogueNodeSO DefeatNode;
    }

    [SerializeField] private string nodeId;
    [SerializeField] private string speaker;
    [TextArea] [SerializeField] private string text;
    [SerializeField] private List<Choice> choices = new List<Choice>();

    public string NodeId => nodeId;

    public DialogueNode ToDomain()
    {
        var domainChoices = new List<DialogueChoice>(choices.Count);
        foreach (var choice in choices)
        {
            var nextId = choice.NextNode != null ? choice.NextNode.nodeId : null;
            var victoryId = choice.VictoryNode != null ? choice.VictoryNode.NodeId : nextId;
            var defeatId = choice.DefeatNode != null ? choice.DefeatNode.NodeId : victoryId;
            domainChoices.Add(new DialogueChoice(choice.Id, choice.Text, nextId, choice.Action, victoryId, defeatId));
        }
        return new DialogueNode(nodeId, speaker, text, domainChoices);
    }
}
