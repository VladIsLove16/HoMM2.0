using System.Collections.Generic;

namespace Adventure.Integration.Battle
{
    public interface IGridConfigurationGateway
    {
        void ApplyConfiguration(IReadOnlyList<GridSlot> playerSlots, IReadOnlyList<GridSlot> enemySlots);
        void LoadBattleScene();
    }

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

        public void SetLineups(IReadOnlyList<UnitStackData> player, IReadOnlyList<UnitStackData> enemy)
        {
            _playerLineup = player;
            _enemyLineup = enemy;
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
