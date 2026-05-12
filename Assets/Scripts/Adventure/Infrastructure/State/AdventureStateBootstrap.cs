using Adventure.Application.Dialog;
using Adventure.Application.Rewards;
using Adventure.Infrastructure.Dialog;
using Adventure.Integration.Battle;
using Adventure.Infrastructure.Players;
using UniRx;
using Zenject;

namespace Adventure.Infrastructure.State
{
    public sealed class AdventureStateBootstrap : IInitializable, System.IDisposable
    {
        private readonly DialogVM _dialogVM;
        private readonly ILocalAdventurePlayerProvider _localPlayerProvider;
        private readonly NpcBehaviorGraphRegistry _behaviorGraphRegistry;
        private readonly IInitialBattleReturnNpc _initialBattleReturnNpc;
        private readonly BattleRewardService _battleRewardService;
        private readonly CompositeDisposable _disposables = new();
        private bool _playerTransformApplied;
        private bool _postBattleFlowCompleted;

        public AdventureStateBootstrap(
            DialogVM dialogVM,
            ILocalAdventurePlayerProvider localPlayerProvider,
            NpcBehaviorGraphRegistry behaviorGraphRegistry,
            [InjectOptional] BattleRewardService battleRewardService = null,
            [InjectOptional] IInitialBattleReturnNpc initialBattleReturnNpc = null)
        {
            _dialogVM = dialogVM;
            _localPlayerProvider = localPlayerProvider;
            _behaviorGraphRegistry = behaviorGraphRegistry;
            _battleRewardService = battleRewardService;
            _initialBattleReturnNpc = initialBattleReturnNpc;
        }

        public void Initialize()
        {
            _localPlayerProvider.PlayerChanged += ApplyPlayerTransform;
            ApplyPlayerTransform();
            Observable.TimerFrame(1)
                .Subscribe(_ => ResumePostBattleFlow())
                .AddTo(_disposables);
        }

        private void ResumePostBattleFlow()
        {
            if (_battleRewardService != null && _battleRewardService.TryShowPendingReward(ResumePostBattleDialog))
            {
                return;
            }

            ResumePostBattleDialog();
        }

        private void ResumePostBattleDialog()
        {
            if (_postBattleFlowCompleted)
            {
                return;
            }

            _postBattleFlowCompleted = true;
            if (!ResumePendingDialog())
            {
                ResumeInitialBattleReturnNpcDialog();
            }
        }

        public void Dispose()
        {
            _localPlayerProvider.PlayerChanged -= ApplyPlayerTransform;
            _disposables.Dispose();
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

        private bool ResumePendingDialog()
        {
            if (!BattleStateCache.TryConsumePendingDialog(
                    out var dialogId,
                    out var resumeNodeId,
                    out ArmyLineupSO lineup,
                    out var outcome,
                    out var returnNpcKey))
            {
                return false;
            }

            if (string.IsNullOrEmpty(dialogId) || string.IsNullOrEmpty(resumeNodeId))
            {
                return false;
            }

            _behaviorGraphRegistry?.SetPendingDialog(dialogId, returnNpcKey);
            var started = _dialogVM?.TryStartDialog(dialogId, lineup, resumeNodeId, returnNpcKey: returnNpcKey) ?? false;
            if (!started)
            {
                _behaviorGraphRegistry?.ClearPending(dialogId);
                return false;
            }

            _behaviorGraphRegistry?.ForceActivate(dialogId, returnNpcKey);
            if (outcome != BattleOutcome.Unknown)
            {
                _behaviorGraphRegistry?.NotifyBattleOutcome(outcome);
            }

            return true;
        }

        private void ResumeInitialBattleReturnNpcDialog()
        {
            if (!BattleStateCache.TryConsumePendingInitialBattleReturnNpcDialog(out var lineup, out var outcome))
            {
                return;
            }

            if (_initialBattleReturnNpc == null)
            {
                UnityEngine.Debug.LogWarning("[AdventureStateBootstrap] Initial battle return NPC is not assigned on AdventureGameplayInstaller.");
                return;
            }

            if (!_initialBattleReturnNpc.TryStartReturnDialog(lineup, outcome))
            {
                UnityEngine.Debug.LogWarning("[AdventureStateBootstrap] Initial battle return NPC could not start return dialog.");
            }
        }
    }
}
