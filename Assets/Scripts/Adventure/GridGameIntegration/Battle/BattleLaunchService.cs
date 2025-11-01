using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Movement;
using Adventure.Infrastructure.State;
using System;
using UnityEngine.SceneManagement;
using Zenject;
using Zenject.SpaceFighter;

namespace Adventure.Integration.Battle
{

    public class BattleLaunchService
    {
        private readonly ArmyFormationResolver _resolver;
        private readonly IGridConfigurationGateway _gateway;
        [Inject]  private MushroomInventoryModel _inventoryModel;
        private readonly PlayerMovementController _playerMovementController;

        public BattleLaunchService(ArmyFormationResolver resolver, IGridConfigurationGateway gateway, PlayerMovementController playerMovementController)
        {
            _resolver = resolver;
            _gateway = gateway;
            _playerMovementController = playerMovementController;
        }


        public void Launch(BattleLaunchContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var enemyArmy = context.EnemyArmy;
            if (enemyArmy == null)
                throw new ArgumentNullException(nameof(context.EnemyArmy), "Enemy army configuration is required to start a battle.");

            var playerSlots = _resolver.ResolveForPlayer(_inventoryModel.GetData());
            var enemySlots = _resolver.ResolveForEnemy(enemyArmy.Convert());
            UnityLogger.Log("BATTLE LAUNCH!! \n player: " + playerSlots.ToString() + "\n enemy: " + enemySlots.ToString());

            _gateway.ApplyConfiguration(playerSlots, enemySlots);
            CaptureAdventureState(context);
            _gateway.LoadBattleScene();
        }

        private void CaptureAdventureState(BattleLaunchContext context)
        {
            if (_playerMovementController != null)
            {
                AdventureStateCache.CapturePlayerTransform(_playerMovementController.transform);
            }

            var currentScene = SceneManager.GetActiveScene();
            if (Enum.TryParse(currentScene.name, out Loader.Scene loaderScene))
            {
                AdventureStateCache.SetReturnScene(loaderScene);
            }
            else
            {
                AdventureStateCache.SetReturnScene(Loader.Scene.Adventure);
            }

            AdventureStateCache.StoreInventorySnapshot(_inventoryModel.GetData());

            if (context.HasDialogContext)
            {
                AdventureStateCache.ScheduleBattleDialog(context.DialogId, context.EnemyArmy, context.VictoryNodeId, context.DefeatNodeId);
            }
        }
    }
}
