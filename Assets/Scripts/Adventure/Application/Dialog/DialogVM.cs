using System;
using Adventure.Domain.Dialog;
using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Dialog;
using Adventure.Integration.Battle;
using Adventure.Settings.ViewModel;
using UniRx;
using Zenject;
using System.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Adventure.Application.Dialog
{
    public sealed class DialogVM : IAdventureGameActiveMenu
    {
        private readonly IDialogRepository _repository;
        private readonly IDialogStateStore _stateStore;
        private readonly PlayerChosenDialogOption _playerChoiceEventChannel;
        private readonly ReactiveProperty<DialogueNode> _currentNode = new ReactiveProperty<DialogueNode>();
        private readonly ReactiveProperty<ArmyLineupSO> _enenyArmy = new ReactiveProperty<ArmyLineupSO>();
        private readonly IArmyLineupFormatter _armyFormatter;
        private DialogueSession _session;
        private string _activeDialogId;
        public IReadOnlyReactiveProperty<bool> IsOpen => _isOpen;
        private readonly ReactiveProperty<bool> _isOpen = new(false);
        [Inject] private BattleLaunchService battleLaunchService;
        public DialogVM(
            IDialogRepository repository,
            IDialogStateStore stateStore,
            PlayerChosenDialogOption playerChoiceEventChannel = null,
            IArmyLineupFormatter armyFormatter = null)
        {
            _repository = repository;
            _stateStore = stateStore;
            _playerChoiceEventChannel = playerChoiceEventChannel;
            _armyFormatter = armyFormatter ?? new DefaultArmyLineupFormatter();
        }
        public IReadOnlyReactiveProperty<DialogueNode> CurrentNode => _currentNode;
        public IReadOnlyReactiveProperty<ArmyLineupSO> EnemyArmy => _enenyArmy;
        public InputMode InputMode => InputMode.Blocked;

        public event Action<DialogueChoiceAction> ChoiceActionTriggered;
        public bool DialogExist(string dialogId)
        {
            return _repository.TryGet(dialogId, out var graph);
        }

        public bool TryStartDialog(string dialogId, ArmyLineupSO armyLineupSO, string startNodeId = null)
        {
            if (string.IsNullOrEmpty(dialogId))
                return false;

            if (!_repository.TryGet(dialogId, out var graph))
                return false;

            _enenyArmy.SetValueAndForceNotify(armyLineupSO);
            _activeDialogId = dialogId;

            if (string.IsNullOrEmpty(startNodeId) && _stateStore != null && _stateStore.TryLoadState(dialogId, out var snapshot) && !string.IsNullOrEmpty(snapshot.CurrentNodeId))
            {
                startNodeId = snapshot.CurrentNodeId;
            }

            var resolver = BuildTextResolver(armyLineupSO);
            _session = new DialogueSession(graph, startNodeId, resolver);
            _session.IsCompleted.Subscribe(OnSessionStateChanged);
            _currentNode.SetValueAndForceNotify(_session.CurrentNode);
            _isOpen.SetValueAndForceNotify(true);
            return true;
        }

        private void OnSessionStateChanged(bool state)
        {
            if (!state)
                return; 
            Close();
        }

        public void SelectChoice(string choiceId)
        {
            if (_session == null)
                throw new InvalidOperationException("Dialog not started");
           
            var currentNode = _session.CurrentNode;
            var selectedChoice = currentNode?.Choices?.FirstOrDefault(c => c.Id == choiceId);
            var action = _session.SelectChoice(choiceId);
            if (action == DialogueChoiceAction.StartBattle)
            {
                StartBattle(selectedChoice);
            }
            else if (action == DialogueChoiceAction.EndDialogue)
            {
                EndDialog();
            }
            else
            {
                ContinueDialog();
            }
            _playerChoiceEventChannel?.SendEventMessage(action);
            ChoiceActionTriggered?.Invoke(action);
        }

        private void ContinueDialog()
        {
            if (_session.IsCompleted.Value)
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

        private void StartBattle(DialogueChoice selectedChoice)
        {
            _currentNode.SetValueAndForceNotify(null);
            var enemyArmy = _enenyArmy.Value;
            var victoryNodeId = selectedChoice?.BattleVictoryNodeId;
            var defeatNodeId = selectedChoice?.BattleDefeatNodeId;
            var context = new BattleLaunchContext(enemyArmy, _activeDialogId, victoryNodeId, defeatNodeId);
            battleLaunchService.Launch(context);
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

        private IDialogueTextResolver BuildTextResolver(ArmyLineupSO armyLineupSO)
        {
            var tokens = new Dictionary<string, string>();
            var armyText = _armyFormatter.Format(armyLineupSO);
            if (!string.IsNullOrEmpty(armyText))
            {
                // Support both legacy <NpcArmy> and your <Army> token.
                tokens["NpcArmy"] = armyText;
                tokens["Army"] = armyText;
            }
            return new DialogueTextResolver(tokens);
        }
    }
}
