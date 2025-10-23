using System.Collections.Generic;

namespace Adventure.Integration.Battle
{
    public interface IGridConfigurationGateway
    {
        void ApplyConfiguration(ArmyFormation playerSlots, ArmyFormation enemySlots);
        void LoadBattleScene();
    }
}
