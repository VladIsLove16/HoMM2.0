using System;

namespace Adventure.Domain.Dialog
{
    public sealed class DialogueSession
    {
        private readonly DialogueGraph _graph;
        private DialogueNode _currentNode;

        public DialogueSession(DialogueGraph graph)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            if (!_graph.TryGetNode(graph.StartNodeId, out _currentNode))
            {
                throw new InvalidOperationException($"Invalid start node id '{graph.StartNodeId}'");
            }
        }

        public DialogueNode CurrentNode => _currentNode;
        public bool IsCompleted { get; private set; }

        public DialogueChoiceAction SelectChoice(string choiceId)
        {
            if (_currentNode == null)
                throw new InvalidOperationException("Dialogue already completed");

            var choice = FindChoice(choiceId);
            if (choice == null)
                throw new InvalidOperationException($"Choice '{choiceId}' not found");

            if (!string.IsNullOrEmpty(choice.NextNodeId) && _graph.TryGetNode(choice.NextNodeId, out var next))
            {
                _currentNode = next;
                return choice.Action;
            }

            // No next node -> dialogue ends
            IsCompleted = true;
            _currentNode = null;
            return choice.Action;
        }

        private DialogueChoice FindChoice(string choiceId)
        {
            foreach (var choice in _currentNode.Choices)
            {
                if (choice.Id == choiceId)
                    return choice;
            }
            return null;
        }
    }
}
