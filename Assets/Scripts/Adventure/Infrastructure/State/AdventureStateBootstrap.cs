using Adventure.Application.Dialog;
using Adventure.Infrastructure.Dialog;
using Adventure.Integration.Battle;
using Adventure.Infrastructure.Players;
using Zenject;

namespace Adventure.Infrastructure.State
{
    public sealed class AdventureStateBootstrap : IInitializable, System.IDisposable
    {
        private readonly DialogVM _dialogVM;
        private readonly ILocalAdventurePlayerProvider _localPlayerProvider;
        private readonly NpcBehaviorGraphRegistry _behaviorGraphRegistry;
        private bool _playerTransformApplied;

        public AdventureStateBootstrap(
            DialogVM dialogVM,
            ILocalAdventurePlayerProvider localPlayerProvider,
            NpcBehaviorGraphRegistry behaviorGraphRegistry)
        {
            _dialogVM = dialogVM;
            _localPlayerProvider = localPlayerProvider;
            _behaviorGraphRegistry = behaviorGraphRegistry;
        }

        public void Initialize()
        {
            _localPlayerProvider.PlayerChanged += ApplyPlayerTransform;
            ApplyPlayerTransform();
            ResumePendingDialog();
        }

        public void Dispose()
        {
            _localPlayerProvider.PlayerChanged -= ApplyPlayerTransform;
        }

        private void ApplyPlayerTransform()
        {
            if (_playerTransformApplied)
                return;

            var playerMovementController = _localPlayerProvider.MovementController;
            if (playerMovementController == null)
            {
                return;
            }

            if (BattleStateCache.TryGetPlayerTransform(out var position, out var rotation))
            {
                var controller = playerMovementController.GetComponent<UnityEngine.CharacterController>();
                if (controller != null)
                {
                    var wasEnabled = controller.enabled;
                    controller.enabled = false;
                    playerMovementController.transform.SetPositionAndRotation(position, rotation);
                    controller.enabled = wasEnabled;
                }
                else
                {
                    playerMovementController.transform.SetPositionAndRotation(position, rotation);
                }

                _playerTransformApplied = true;
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
            }
        }
    }
}
