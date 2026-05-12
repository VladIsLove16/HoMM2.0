using System.Collections.Generic;
using Adventure.Domain.Dialog;
using UnityEngine;

[CreateAssetMenu(menuName = "Adventure/Dialogue/Graph", fileName = "DialogueGraph")]
public sealed class DialogueGraphSO : ScriptableObject
{
    [SerializeField] private DialogueNodeSO startNode;
    [SerializeField] private List<DialogueNodeSO> additionalNodes = new List<DialogueNodeSO>();

    public string Id => name;
    public string StartNodeId => startNode != null ? startNode.NodeId : string.Empty;

    public DialogueGraph ToDomain()
    {
        var nodes = new List<DialogueNode>();
        if (startNode != null)
        {
            nodes.Add(startNode.ToDomain());
        }
        foreach (var node in additionalNodes)
        {
            if (node != null)
            {
                nodes.Add(node.ToDomain());
            }
        }

        return new DialogueGraph(startNode != null ? startNode.NodeId : string.Empty, nodes);
    }
}
