using System.Collections.Generic;

namespace Adventure.Integration.Battle
{
    public interface IGridConfigurationGateway
    {
        void ApplyConfiguration(IReadOnlyList<GridSlot> playerSlots, IReadOnlyList<GridSlot> enemySlots);
        void LoadBattleScene();
    }
}
