using System;
using Adventure.Domain.Dialog;
using Adventure.Domain.Inventory;
using Adventure.Domain.Progression;
using Adventure.Infrastructure.Dialog;
using Adventure.Integration.Battle;
using Adventure.Settings.ViewModel;
using UniRx;
using Zenject;
using System.Linq;
using System.Collections.Generic;

namespace Adventure.Application.Dialog
{
    public sealed class DialogVM : IAdventureGameActiveMenu
    {
        private readonly IDialogRepository _repository;
        private readonly IDialogStateStore _stateStore;
        private readonly ReactiveProperty<DialogueNode> _currentNode = new ReactiveProperty<DialogueNode>();
        private readonly ReactiveProperty<ArmyLineupSO> _enenyArmy = new ReactiveProperty<ArmyLineupSO>();
        private readonly IArmyLineupFormatter _armyFormatter;
        private readonly IStoryFlagsService _storyFlagsService;
        private DialogueSession _session;
        private string _activeDialogId;
        public string ActiveDialogId => _activeDialogId;
        public IReadOnlyReactiveProperty<bool> IsOpen => _isOpen;
        private readonly ReactiveProperty<bool> _isOpen = new(false);
        [Inject] private BattleLaunchService battleLaunchService;
        public DialogVM(
            IDialogRepository repository,
            IDialogStateStore stateStore,
            IStoryFlagsService storyFlagsService,
            IArmyLineupFormatter armyFormatter = null)
        {
            _repository = repository;
            _stateStore = stateStore;
            _storyFlagsService = storyFlagsService;
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
            RefreshCurrentNode();
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

            var selectedChoice = _currentNode.Value?.Choices?.FirstOrDefault(c => c.Id == choiceId);
            if (selectedChoice == null || !selectedChoice.IsAvailable)
                throw new InvalidOperationException($"Choice '{choiceId}' not found");

            _storyFlagsService?.SetMany(selectedChoice.GrantedFlagIds);

            var action = selectedChoice.Action;
            ChoiceActionTriggered?.Invoke(action);

            if (action == DialogueChoiceAction.StartBattle)
            {
                StartBattle(selectedChoice);
            }
            else if (action == DialogueChoiceAction.StartOnlineBattle)
            {
                StartOnlineBattle(selectedChoice);
            }
            else if (action == DialogueChoiceAction.EndDialogue)
            {
                _session.SelectChoice(choiceId);
                if (_isOpen.Value)
                {
                    Close();
                }
            }
            else
            {
                _session.SelectChoice(choiceId);
                ContinueDialog();
            }
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
                RefreshCurrentNode();
            }
        }

        private void StartBattle(DialogueChoice selectedChoice)
        {
            var enemyArmy = _enenyArmy.Value;
            var victoryNodeId = selectedChoice?.BattleVictoryNodeId;
            var defeatNodeId = selectedChoice?.BattleDefeatNodeId;
            var context = new BattleLaunchContext(enemyArmy, _activeDialogId, victoryNodeId, defeatNodeId);
            battleLaunchService.Launch(context);
        }

        private void StartOnlineBattle(DialogueChoice selectedChoice)
        {
            var enemyArmy = _enenyArmy.Value;
            var victoryNodeId = selectedChoice?.BattleVictoryNodeId;
            var defeatNodeId = selectedChoice?.BattleDefeatNodeId;
            var context = new BattleLaunchContext(enemyArmy, _activeDialogId, victoryNodeId, defeatNodeId);
            battleLaunchService.LaunchOnline(context);
        }

        public void Close()
        {
            if (!_isOpen.Value && _session == null && _currentNode.Value == null && _enenyArmy.Value == null && string.IsNullOrEmpty(_activeDialogId))
            {
                return;
            }

            var dialogId = _activeDialogId;
            _session = null;
            _currentNode.SetValueAndForceNotify(null);
            _enenyArmy.SetValueAndForceNotify(null);

            if (!string.IsNullOrEmpty(dialogId))
            {
                _stateStore?.ClearState(dialogId);
            }

            _isOpen.SetValueAndForceNotify(false);
            _activeDialogId = null;
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

        private void RefreshCurrentNode()
        {
            _currentNode.SetValueAndForceNotify(BuildPresentedNode(_session?.CurrentNode));
        }

        private DialogueNode BuildPresentedNode(DialogueNode sourceNode)
        {
            if (sourceNode == null)
            {
                return null;
            }

            if (sourceNode.Choices == null || sourceNode.Choices.Count == 0)
            {
                return sourceNode;
            }

            var presentedChoices = new List<DialogueChoice>(sourceNode.Choices.Count);
            foreach (var choice in sourceNode.Choices)
            {
                if (choice == null)
                {
                    continue;
                }

                var isAvailable = _storyFlagsService?.HasAll(choice.RequiredFlagIds) ?? true;
                if (!isAvailable && choice.HideIfLocked)
                {
                    continue;
                }

                var choiceText = isAvailable || string.IsNullOrWhiteSpace(choice.LockedText)
                    ? choice.Text
                    : choice.LockedText;

                presentedChoices.Add(choice.WithPresentation(choiceText, isAvailable));
            }

            return new DialogueNode(sourceNode.Id, sourceNode.Speaker, sourceNode.Text, presentedChoices);
        }
    }
}
