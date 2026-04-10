using Adventure.Application.Dialog;
using Adventure.Domain.Dialog;
using UniRx;
using Zenject;

namespace Adventure.Infrastructure.Dialog
{
    public sealed class NpcDialogueBehaviorMediator : IInitializable, System.IDisposable
    {
        private readonly DialogVM _dialogVM;
        private readonly NpcBehaviorGraphRegistry _registry;
        private readonly CompositeDisposable _subscriptions = new();

        public NpcDialogueBehaviorMediator(DialogVM dialogVM, NpcBehaviorGraphRegistry registry)
        {
            _dialogVM = dialogVM;
            _registry = registry;
        }

        public void Initialize()
        {
            _dialogVM.IsOpen.Subscribe(OnDialogOpenStateChanged).AddTo(_subscriptions);
            _dialogVM.CurrentNode.Subscribe(OnNodeChanged).AddTo(_subscriptions);
            _dialogVM.ChoiceActionTriggered += OnChoiceActionTriggered;
        }

        public void Dispose()
        {
            _dialogVM.ChoiceActionTriggered -= OnChoiceActionTriggered;
            _subscriptions.Dispose();
        }

        private void OnDialogOpenStateChanged(bool isOpen)
        {
            if (isOpen)
            {
                _registry.NotifyDialogOpened();
            }
            else
            {
                _registry.NotifyDialogClosed();
            }
        }

        private void OnNodeChanged(DialogueNode node)
        {
            var hasChoices = node?.Choices != null && node.Choices.Count > 0;
            _registry.NotifyChoiceWindowState(hasChoices);
        }

        private void OnChoiceActionTriggered(DialogueChoiceAction action)
        {
            if (action == DialogueChoiceAction.StartBattle || action == DialogueChoiceAction.StartOnlineBattle)
            {
                _registry.NotifyBattleRequested();
            }
        }
    }
}
