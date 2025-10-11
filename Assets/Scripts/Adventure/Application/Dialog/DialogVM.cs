using System;
using Adventure.Domain.Dialog;
using Adventure.Integration.Battle;
using UniRx;
using Zenject;

namespace Adventure.Application.Dialog
{
    public sealed class DialogVM
    {
        private readonly IDialogRepository _repository;
        private readonly IDialogStateStore _stateStore;
        private readonly ReactiveProperty<DialogueNode> _currentNode = new ReactiveProperty<DialogueNode>();
        private readonly ReactiveProperty<ArmyLineupSO> _enenyArmy = new ReactiveProperty<ArmyLineupSO>();
        private DialogueSession _session;
        private string _activeDialogId;
        [Inject]   private BattleLaunchService battleLaunchService;
        [Inject]   private BattlePreparationService battlePreparationService;
        public DialogVM(IDialogRepository repository, IDialogStateStore stateStore)
        {
            _repository = repository;
            _stateStore = stateStore;
        }

        public IReadOnlyReactiveProperty<DialogueNode> CurrentNode => _currentNode;
        public IReadOnlyReactiveProperty<ArmyLineupSO> EnemyArmy => _enenyArmy;
        public event Action<DialogueChoiceAction> ChoiceActionTriggered;

        public bool TryStartDialog(string dialogId, ArmyLineupSO armyLineupSO)
        {
            if (string.IsNullOrEmpty(dialogId))
                return false;

            if (!_repository.TryGet(dialogId, out var graph))
                return false;

            _enenyArmy.SetValueAndForceNotify(armyLineupSO);
            _activeDialogId = dialogId;
            _session = new DialogueSession(graph);
            _currentNode.Value = _session.CurrentNode;
            return true;
        }

        public void SelectChoice(string choiceId)
        {
            if (_session == null)
                throw new InvalidOperationException("Dialog not started");

            var action = _session.SelectChoice(choiceId);
            if (_session.IsCompleted)
            {
                _stateStore?.ClearState(_activeDialogId);
                _currentNode.Value = null;
            }
            else
            {
                _stateStore?.SaveState(new DialogStateSnapshot(_activeDialogId, _session.CurrentNode.Id));
                _currentNode.Value = _session.CurrentNode;
            }
            if (action == DialogueChoiceAction.StartBattle)
            {
                _currentNode.SetValueAndForceNotify(null);
                var battleArmies =  battlePreparationService.BuildArmies();
                battleLaunchService.SetEnemyLineup(battleArmies.EnemyUnits);
                battleLaunchService.SetPlayerLineup(battleArmies.PlayerUnits);
                battleLaunchService.Launch();
            }
            else if (action == DialogueChoiceAction.EndDialogue)
            {
                _currentNode.SetValueAndForceNotify(null);
            }
            ChoiceActionTriggered?.Invoke(action);
        }

        public void Cancel()
        {
            _session = null;
            _currentNode.Value = null;
            if (!string.IsNullOrEmpty(_activeDialogId))
            {
                _stateStore?.ClearState(_activeDialogId);
            }
        }
    }
}
