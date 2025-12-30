using Adventure.Application.Dialog;
using Adventure.Infrastructure.Dialog;
using Adventure.Integration.Battle;
using Adventure.Infrastructure.Movement;
using Zenject;

namespace Adventure.Infrastructure.State
{
    public sealed class AdventureStateBootstrap : IInitializable
    {
        private readonly DialogVM _dialogVM;
        private readonly PlayerMovementController _playerMovementController;
        private readonly NpcBehaviorGraphRegistry _behaviorGraphRegistry;
        private readonly BattleFinishedChannel _battleFinishedChannel;

        public AdventureStateBootstrap(
            DialogVM dialogVM,
            PlayerMovementController playerMovementController,
            NpcBehaviorGraphRegistry behaviorGraphRegistry,
            BattleFinishedChannel battleFinishedChannel = null)
        {
            _dialogVM = dialogVM;
            _playerMovementController = playerMovementController;
            _behaviorGraphRegistry = behaviorGraphRegistry;
            _battleFinishedChannel = battleFinishedChannel;
        }

        public void Initialize()
        {
            ApplyPlayerTransform();
            ResumePendingDialog();
        }

        private void ApplyPlayerTransform()
        {
            if (_playerMovementController == null)
            {
                return;
            }

            if (BattleStateCache.TryGetPlayerTransform(out var position, out var rotation))
            {
                var controller = _playerMovementController.GetComponent<UnityEngine.CharacterController>();
                if (controller != null)
                {
                    var wasEnabled = controller.enabled;
                    controller.enabled = false;
                    _playerMovementController.transform.SetPositionAndRotation(position, rotation);
                    controller.enabled = wasEnabled;
                }
                else
                {
                    _playerMovementController.transform.SetPositionAndRotation(position, rotation);
                }
            }
        }

        private void ResumePendingDialog()
        {
            if (!BattleStateCache.TryConsumePendingDialog(out var dialogId, out var resumeNodeId, out ArmyLineupSO lineup, out var outcome))
            {
                return;
            }

            if (string.IsNullOrEmpty(dialogId) || string.IsNullOrEmpty(resumeNodeId))
            {
                return;
            }

            _behaviorGraphRegistry?.SetPendingDialog(dialogId);
            var started = _dialogVM?.TryStartDialog(dialogId, lineup, resumeNodeId) ?? false;
            if (!started)
            {
                _behaviorGraphRegistry?.ClearPending(dialogId);
                return;
            }

            _behaviorGraphRegistry?.ForceActivate(dialogId);
            if (outcome != BattleOutcome.Unknown)
            {
                _behaviorGraphRegistry?.NotifyBattleOutcome(outcome);
                _battleFinishedChannel?.SendEventMessage(outcome);
            }
        }
    }
}
