using System;
using UniRx;

namespace Adventure.Domain.Dialog
{
    public sealed class DialogueSession
    {
        private readonly DialogueGraph _graph;
        private DialogueNode _currentNode;

        public DialogueSession(DialogueGraph graph, string startNodeId = null)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            var nodeId = string.IsNullOrEmpty(startNodeId) ? graph.StartNodeId : startNodeId;
            if (!_graph.TryGetNode(nodeId, out _currentNode))
            {
                if (!_graph.TryGetNode(graph.StartNodeId, out _currentNode))
                {
                    throw new InvalidOperationException($"Invalid start node id '{nodeId}'");
                }
            }
        }

        public DialogueNode CurrentNode => _currentNode;
        public ReactiveProperty<bool> IsCompleted = new(false);

        public DialogueChoiceAction SelectChoice(string choiceId)
        {
            if (IsCompleted.Value)
                throw new InvalidOperationException("Dialogue already completed");

            var choice = FindChoice(choiceId);
            if (choice == null)
                throw new InvalidOperationException($"Choice '{choiceId}' not found");
            if (FindNextNode(choice, out DialogueNode next))
            {
                return ProceedNextNode(choice, next);
            }

            CompleteSession();
            return choice.Action;
        }

        private DialogueChoiceAction ProceedNextNode(DialogueChoice choice, DialogueNode next)
        {
            _currentNode = next;
            return choice.Action;
        }

        private void CompleteSession()
        {
            IsCompleted.SetValueAndForceNotify(true);
            _currentNode = null;
        }

        private bool FindNextNode(DialogueChoice choice, out DialogueNode next)
        {
            if (choice.NextNodeId == null)
            {
                next = null; 
                return false;
            }
            bool isNextNode = _graph.TryGetNode(choice.NextNodeId, out next);
            return isNextNode && !string.IsNullOrEmpty(choice.NextNodeId);
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
