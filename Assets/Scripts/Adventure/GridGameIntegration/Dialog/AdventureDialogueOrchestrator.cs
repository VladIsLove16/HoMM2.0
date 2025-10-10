using Adventure.Domain.Dialog;
using Adventure.Integration.Battle;
using Adventure.Application.Dialog;
using Adventure.Presentation.Dialog;
using UnityEngine;
using System;

namespace Adventure.Infrastructure.Dialog
{
    public sealed class AdventureDialogueOrchestrator : MonoBehaviour
    {
        [SerializeField] private DialogueUIView view;
        private ArmyLineupSO currentEnemyArmyLineup;
        
        private Application.Dialog.DialogVM _dialogService;
        private BattleLaunchService _battleLaunchService;
        private Presentation.Dialog.DialogService _viewModel;

        public void Construct(Application.Dialog.DialogVM dialogService, BattleLaunchService battleLaunchService )
        {
            _dialogService = dialogService;
            _battleLaunchService = battleLaunchService;
            _viewModel = new Presentation.Dialog.DialogService(dialogService);
            if (view != null)
                view.Construct(_viewModel);

            _dialogService.ChoiceActionTriggered += HandleChoiceAction;
        }

        public bool StartDialog(string dialogId, ArmyLineupSO currentArmyLineup)
        {
            this.currentEnemyArmyLineup = currentArmyLineup;
            return _viewModel.TryStartDialog(dialogId);
        }

        public void ApplyLineup(ArmyLineupSO lineup)
        {
            if (lineup == null) return;
            _battleLaunchService.SetEnemyLineup(lineup.Convert());
        }

        private void HandleChoiceAction(DialogueChoiceAction action)
        {
            switch (action)
            {
                case DialogueChoiceAction.StartBattle:
                    _battleLaunchService.SetPlayerLineup();
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
