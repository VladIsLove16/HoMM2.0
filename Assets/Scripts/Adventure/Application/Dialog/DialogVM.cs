using System;
using Adventure.Domain.Dialog;
using Adventure.Domain.Inventory;
using Adventure.Integration.Battle;
using Adventure.Settings.ViewModel;
using UniRx;
using Zenject;

namespace Adventure.Application.Dialog
{
    public sealed class DialogVM : IActiveMenu
    {
        private readonly IDialogRepository _repository;
        private readonly IDialogStateStore _stateStore;
        private readonly ReactiveProperty<DialogueNode> _currentNode = new ReactiveProperty<DialogueNode>();
        private readonly ReactiveProperty<ArmyLineupSO> _enenyArmy = new ReactiveProperty<ArmyLineupSO>();
        private DialogueSession _session;
        private string _activeDialogId;
        public IReadOnlyReactiveProperty<bool> IsOpen => _isOpen;
        private readonly ReactiveProperty<bool> _isOpen = new(false);
        [Inject] private BattleLaunchService battleLaunchService;
        public DialogVM(IDialogRepository repository, IDialogStateStore stateStore)
        {
            _repository = repository;
            _stateStore = stateStore;
        }
        public IReadOnlyReactiveProperty<DialogueNode> CurrentNode => _currentNode;
        public IReadOnlyReactiveProperty<ArmyLineupSO> EnemyArmy => _enenyArmy;

        public InputMode InputMode => InputMode.Blocked;

        public event Action<DialogueChoiceAction> ChoiceActionTriggered;
        public bool DialogExist(string dialogId)
        {
            return _repository.TryGet(dialogId, out var graph);
        }
        public bool TryStartDialog(string dialogId, ArmyLineupSO armyLineupSO)
        {
            if (string.IsNullOrEmpty(dialogId))
                return false;

            if (!_repository.TryGet(dialogId, out var graph))
                return false;

            _enenyArmy.SetValueAndForceNotify(armyLineupSO);
            _activeDialogId = dialogId;
            _session = new DialogueSession(graph);
            _currentNode.SetValueAndForceNotify(_session.CurrentNode);
            _isOpen.SetValueAndForceNotify(true);
            return true;
        }

        public void SelectChoice(string choiceId)
        {
            if (_session == null)
                throw new InvalidOperationException("Dialog not started");
           
            var action = _session.SelectChoice(choiceId);
            if (action == DialogueChoiceAction.StartBattle)
            {
                StartBattle();
            }
            else if (action == DialogueChoiceAction.EndDialogue)
            {
                EndDialog();
            }
            else
            {
                ContinueDialog();
            }
            ChoiceActionTriggered?.Invoke(action);
        }

        private void ContinueDialog()
        {
            if (_session.IsCompleted)
            {
                _stateStore?.ClearState(_activeDialogId);
                _currentNode.Value = null;
            }
            else
            {
                _stateStore?.SaveState(new DialogStateSnapshot(_activeDialogId, _session.CurrentNode.Id));
                _currentNode.SetValueAndForceNotify(_session.CurrentNode);
            }
        }

        private void StartBattle()
        {
            _currentNode.SetValueAndForceNotify(null);
            battleLaunchService.Launch(_enenyArmy.Value);
        }

        private void EndDialog()
        {
            _isOpen.SetValueAndForceNotify(false);
            _session = null;
            _currentNode.SetValueAndForceNotify(null);
            _enenyArmy.SetValueAndForceNotify(null);
            _activeDialogId = null;
        }
        public void Close()
        {
            _session = null;
            _currentNode.SetValueAndForceNotify(null);
            if (!string.IsNullOrEmpty(_activeDialogId))
            {
                _stateStore?.ClearState(_activeDialogId);
            }
            _isOpen.SetValueAndForceNotify(false);
        }

        public void Open()
        {
            _isOpen.SetValueAndForceNotify(true);
        }
    }
}
