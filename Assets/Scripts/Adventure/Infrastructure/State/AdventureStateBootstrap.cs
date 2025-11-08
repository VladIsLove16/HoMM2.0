using Adventure.Application.Dialog;
using Adventure.Integration.Battle;
using Adventure.Infrastructure.Movement;
using Zenject;

namespace Adventure.Infrastructure.State
{
    public sealed class AdventureStateBootstrap : IInitializable
    {
        private readonly DialogVM _dialogVM;
        private readonly PlayerMovementController _playerMovementController;

        public AdventureStateBootstrap(DialogVM dialogVM, PlayerMovementController playerMovementController)
        {
            _dialogVM = dialogVM;
            _playerMovementController = playerMovementController;
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
            if (!BattleStateCache.TryConsumePendingDialog(out var dialogId, out var resumeNodeId, out ArmyLineupSO lineup, out var _))
            {
                return;
            }

            if (string.IsNullOrEmpty(dialogId) || string.IsNullOrEmpty(resumeNodeId))
            {
                return;
            }

            _dialogVM?.TryStartDialog(dialogId, lineup, resumeNodeId);
        }
    }
}
