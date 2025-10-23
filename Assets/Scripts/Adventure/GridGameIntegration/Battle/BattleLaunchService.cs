using Adventure.Domain.Inventory;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Zenject;
using Zenject.SpaceFighter;

namespace Adventure.Integration.Battle
{

    public class BattleLaunchService
    {
        private readonly ArmyFormationResolver _resolver;
        private readonly IGridConfigurationGateway _gateway;
        [Inject]  private MushroomInventoryModel _inventoryModel;
        public BattleLaunchService(ArmyFormationResolver resolver, IGridConfigurationGateway gateway)
        {
            _resolver = resolver;
            _gateway = gateway;
        }


        public void Launch(ArmyLineupSO enemyArmy)
        {
            var playerSlots = _resolver.ResolveForPlayer(_inventoryModel.GetData());
            var enemySlots = _resolver.ResolveForEnemy(enemyArmy.Convert());
            UnityLogger.Log("BATTLE LAUNCH!! \n player: " + playerSlots.ToString() + "\n enemy: " + enemySlots.ToString());

            _gateway.ApplyConfiguration(playerSlots, enemySlots);
            _gateway.LoadBattleScene();
        }
    }
}
