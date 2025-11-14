using System;
using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Movement;
using Adventure.Infrastructure.State;
using UnityEngine;

namespace Adventure.Integration.Battle
{
    /// <summary>
    /// Coordinates the transition from the adventure scene into the tactical grid battle.
    /// </summary>
    public sealed class BattleLaunchService
    {
        private readonly MushroomInventoryModel _inventory;
        private readonly PlayerMovementController _playerMovement;
        private readonly IGridConfigurationGateway _gridGateway;
        private readonly ArmyFormationResolver _formationResolver;

        public BattleLaunchService(
            MushroomInventoryModel inventory,
            PlayerMovementController playerMovement,
            IGridConfigurationGateway gridGateway,
            ArmyFormationResolver formationResolver)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _playerMovement = playerMovement;
            _gridGateway = gridGateway ?? throw new ArgumentNullException(nameof(gridGateway));
            _formationResolver = formationResolver ?? throw new ArgumentNullException(nameof(formationResolver));
        }

        public Action PrepareLaunch(BattleLaunchContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var playerStacks = _inventory.GetData();
            var enemyStacks = context.EnemyArmy != null
                ? context.EnemyArmy.Convert()
                : Array.Empty<UnitStackData>();

            if (_playerMovement != null)
            {
                BattleStateCache.CapturePlayerTransform(_playerMovement.transform);
            }

            BattleStateCache.StoreInventorySnapshot(playerStacks);
            BattleStateCache.ScheduleBattleDialog(context.DialogId, context.EnemyArmy, context.VictoryNodeId, context.DefeatNodeId);

            var payload = new BattleSetupPayload(
                new BattleArmies(playerStacks, enemyStacks),
                context.ReturnScene);

            _gridGateway.PrepareBattle(payload, _formationResolver);

            return () => Loader.Load(_gridGateway.BattleScene);
        }

        public void Launch(BattleLaunchContext context)
        {
            var finalize = PrepareLaunch(context);
            finalize?.Invoke();
        }
    }
}
