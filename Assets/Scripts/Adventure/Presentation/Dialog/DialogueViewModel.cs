using Adventure.Application.Dialog;
using Adventure.Domain.Dialog;
using UniRx;

namespace Adventure.Presentation.Dialog
{
    public sealed class DialogueViewModel
    {
        private readonly DialogService _dialogService;

        public DialogueViewModel(DialogService dialogService)
        {
            _dialogService = dialogService;
            CurrentNode = _dialogService.CurrentNode;
        }

        public IReadOnlyReactiveProperty<DialogueNode> CurrentNode { get; }

        public void SelectChoice(string choiceId)
        {
            _dialogService.SelectChoice(choiceId);
        }

        public bool TryStartDialog(string dialogId) => _dialogService.TryStartDialog(dialogId);
        public void Cancel() => _dialogService.Cancel();
    }
}
