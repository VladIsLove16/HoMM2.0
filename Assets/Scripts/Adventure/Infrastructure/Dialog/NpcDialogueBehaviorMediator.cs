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
        private string _lastKnownDialogId;

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
            var dialogId = SyncActiveBridge();

            if (isOpen)
            {
                _registry.NotifyDialogOpened(dialogId);
            }
            else
            {
                _registry.NotifyDialogClosed(dialogId ?? _lastKnownDialogId);
                _lastKnownDialogId = null;
            }
        }

        private void OnNodeChanged(DialogueNode node)
        {
            var dialogId = SyncActiveBridge();
            var hasChoices = node?.Choices != null && node.Choices.Count > 0;
            _registry.NotifyChoiceWindowState(hasChoices, dialogId ?? _lastKnownDialogId);
        }

        private void OnChoiceActionTriggered(DialogueChoiceAction action)
        {
            var dialogId = SyncActiveBridge();
            _registry.NotifyChoiceAction(action, dialogId ?? _lastKnownDialogId);

            if (action == DialogueChoiceAction.StartBattle || action == DialogueChoiceAction.StartOnlineBattle)
            {
                _registry.NotifyBattleRequested(dialogId ?? _lastKnownDialogId);
            }
        }

        private string SyncActiveBridge()
        {
            var dialogId = _dialogVM.ActiveDialogId;
            if (!string.IsNullOrEmpty(dialogId))
            {
                _lastKnownDialogId = dialogId;
                _registry.ForceActivate(dialogId, _dialogVM.ActiveReturnNpcKey);
            }

            return dialogId;
        }
    }
}
