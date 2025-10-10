using System.Collections.Generic;

namespace Adventure.Domain.Dialog
{
    public sealed class DialogueGraph
    {
        private readonly Dictionary<string, DialogueNode> _nodes;
        public string StartNodeId { get; }

        public DialogueGraph(string startNodeId, IEnumerable<DialogueNode> nodes)
        {
            StartNodeId = startNodeId;
            _nodes = new Dictionary<string, DialogueNode>();
            foreach (var node in nodes)
            {
                if (node != null && !string.IsNullOrEmpty(node.Id))
                {
                    _nodes[node.Id] = node;
                }
            }
        }

        public bool TryGetNode(string nodeId, out DialogueNode node) => _nodes.TryGetValue(nodeId, out node);
    }
}
