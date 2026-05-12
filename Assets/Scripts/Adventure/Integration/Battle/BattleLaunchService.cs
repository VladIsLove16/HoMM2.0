using System;
using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Players;
using Adventure.Infrastructure.State;
using Adventure.Multiplayer;
using Unity.Netcode;
using UnityEngine;

namespace Adventure.Integration.Battle
{
    /// <summary>
    /// Coordinates the transition from the adventure scene into the tactical grid battle.
    /// </summary>
    public sealed class BattleLaunchService
    {
        private readonly MushroomInventoryModel _inventory;
        private readonly ILocalAdventurePlayerProvider _localPlayerProvider;
        private readonly IGridConfigurationGateway _gridGateway;
        private readonly ArmyFormationResolver _formationResolver;
        private readonly OnlineBattleLaunchService _onlineBattleLaunchService;

        public BattleLaunchService(
            MushroomInventoryModel inventory,
            ILocalAdventurePlayerProvider localPlayerProvider,
            IGridConfigurationGateway gridGateway,
            ArmyFormationResolver formationResolver,
            OnlineBattleLaunchService onlineBattleLaunchService = null)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _localPlayerProvider = localPlayerProvider;
            _gridGateway = gridGateway ?? throw new ArgumentNullException(nameof(gridGateway));
            _formationResolver = formationResolver ?? throw new ArgumentNullException(nameof(formationResolver));
            _onlineBattleLaunchService = onlineBattleLaunchService;
        }

        public Action PrepareLaunch(BattleLaunchContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var playerStacks = _inventory.GetData();
            var enemyStacks = context.EnemyArmy != null
                ? context.EnemyArmy.Convert()
                : Array.Empty<UnitStackData>();

            var playerMovement = _localPlayerProvider.MovementController;
            if (playerMovement != null)
            {
                BattleStateCache.CapturePlayerTransform(playerMovement.transform);
            }

            BattleStateCache.StoreInventorySnapshot(playerStacks);
            BattleStateCache.ScheduleBattleDialog(
                context.DialogId,
                context.EnemyArmy,
                context.VictoryNodeId,
                context.DefeatNodeId,
                context.ReturnNpcKey,
                context.FallbackNodeId);
            BattleStateCache.ScheduleBattleReward(context.VictoryReward != null
                ? context.VictoryReward.Convert()
                : Array.Empty<UnitStackData>(),
                context.VictoryRewardId);

            var payload = new BattleSetupPayload(
                new BattleArmies(playerStacks, enemyStacks),
                context.ReturnScene,
                _gridGateway.PlayerTeam,
                _gridGateway.PlayerTeam);

            _gridGateway.PrepareBattle(payload, _formationResolver);

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                Debug.LogError("[BattleLaunchService] Online battle handoff from Adventure is not configured yet. Use a dedicated network battle session transition.");
                return null;
            }

            return () => SceneLoader.Load(_gridGateway.BattleScene);
        }

        public void Launch(BattleLaunchContext context)
        {
            var finalize = PrepareLaunch(context);
            finalize?.Invoke();
        }

        public bool LaunchOnline(BattleLaunchContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            {
                Debug.LogWarning("[BattleLaunchService] StartOnlineBattle requested outside of a network session.");
                return false;
            }

            if (_onlineBattleLaunchService == null)
            {
                Debug.LogError("[BattleLaunchService] OnlineBattleLaunchService is not available.");
                return false;
            }

            return _onlineBattleLaunchService.TryLaunch(context);
        }
    }
}
