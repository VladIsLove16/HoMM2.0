using System;

namespace Adventure.Integration.Battle
{
    /// <summary>
    /// Carries the data required to transition from the adventure scene into a grid battle.
    /// </summary>
    public sealed class BattleLaunchContext
    {
        public BattleLaunchContext(
            ArmyLineupSO enemyArmy,
            string dialogId,
            string victoryNodeId,
            string defeatNodeId,
            SceneLoader.Scene returnScene = SceneLoader.Scene.Adventure)
        {
            EnemyArmy = enemyArmy;
            DialogId = dialogId;
            VictoryNodeId = victoryNodeId;
            DefeatNodeId = defeatNodeId;
            ReturnScene = returnScene;
        }

        public ArmyLineupSO EnemyArmy { get; }
        public string DialogId { get; }
        public string VictoryNodeId { get; }
        public string DefeatNodeId { get; }
        public SceneLoader.Scene ReturnScene { get; }

        public bool HasDialog => !string.IsNullOrWhiteSpace(DialogId);
    }
}
