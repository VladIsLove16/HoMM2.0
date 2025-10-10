using Adventure.Domain.Dialog;
using Adventure.Integration.Battle;
using Adventure.Application.Dialog;
using Adventure.Presentation.Dialog;
using UnityEngine;

namespace Adventure.Infrastructure.Dialog
{
    public sealed class AdventureDialogueOrchestrator : MonoBehaviour
    {
        [SerializeField] private DialogueUIView view;
        [SerializeField] private ArmyLineupSO defaultLineup;
        
        private DialogService _dialogService;
        private BattleLaunchService _battleLaunchService;
        private DialogueViewModel _viewModel;

        public void Construct(DialogService dialogService, BattleLaunchService battleLaunchService)
        {
            _dialogService = dialogService;
            _battleLaunchService = battleLaunchService;
            _viewModel = new DialogueViewModel(dialogService);
            if (view != null)
                view.Construct(_viewModel);

            _dialogService.ChoiceActionTriggered += HandleChoiceAction;

            ApplyLineup(defaultLineup);
        }

        public bool StartDialog(string dialogId)
        {
            return _viewModel.TryStartDialog(dialogId);
        }

        public void ApplyLineup(ArmyLineupSO lineup)
        {
            if (lineup == null) return;
            _battleLaunchService.SetLineups(lineup.GetPlayerLineup(), lineup.GetEnemyLineup());
        }

        private void HandleChoiceAction(DialogueChoiceAction action)
        {
            switch (action)
            {
                case DialogueChoiceAction.StartBattle:
                    _battleLaunchService.Launch();
                    break;
                case DialogueChoiceAction.EndDialogue:
                    _viewModel.Cancel();
                    break;
            }
        }

        private void OnDestroy()
        {
            if (_dialogService != null)
                _dialogService.ChoiceActionTriggered -= HandleChoiceAction;
        }
    }
}
