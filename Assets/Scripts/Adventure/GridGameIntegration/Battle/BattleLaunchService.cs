using System.Collections.Generic;

namespace Adventure.Integration.Battle
{

    public sealed class BattleLaunchService
    {
        private readonly ArmyFormationResolver _resolver;
        private readonly IGridConfigurationGateway _gateway;
        private IReadOnlyList<UnitStackData> _playerLineup;
        private IReadOnlyList<UnitStackData> _enemyLineup;

        public BattleLaunchService(ArmyFormationResolver resolver, IGridConfigurationGateway gateway)
        {
            _resolver = resolver;
            _gateway = gateway;
        }

        public void SetEnemyLineup(IReadOnlyList<UnitStackData> enemy)
        {
            _enemyLineup = enemy;
        }
        public void SetPlayerLineup(IReadOnlyList<UnitStackData> player )
        {
            _playerLineup = player;
        }

        public void Launch()
        {
            var playerSlots = _resolver.ResolveForPlayer(_playerLineup ?? new List<UnitStackData>());
            var enemySlots = _resolver.ResolveForEnemy(_enemyLineup ?? new List<UnitStackData>());
            _gateway.ApplyConfiguration(playerSlots, enemySlots);
            _gateway.LoadBattleScene();
        }
    }
}
