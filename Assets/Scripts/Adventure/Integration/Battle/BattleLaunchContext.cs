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
            SceneLoader.Scene returnScene = SceneLoader.Scene.Adventure,
            ArmyLineupSO victoryReward = null,
            string victoryRewardId = null,
            string returnNpcKey = null,
            string fallbackNodeId = null)
        {
            EnemyArmy = enemyArmy;
            DialogId = dialogId;
            VictoryNodeId = victoryNodeId;
            DefeatNodeId = defeatNodeId;
            ReturnScene = returnScene;
            VictoryReward = victoryReward;
            VictoryRewardId = victoryRewardId;
            ReturnNpcKey = returnNpcKey;
            FallbackNodeId = fallbackNodeId;
        }

        public ArmyLineupSO EnemyArmy { get; }
        public ArmyLineupSO VictoryReward { get; }
        public string VictoryRewardId { get; }
        public string DialogId { get; }
        public string VictoryNodeId { get; }
        public string DefeatNodeId { get; }
        public SceneLoader.Scene ReturnScene { get; }
        public string ReturnNpcKey { get; }
        public string FallbackNodeId { get; }

        public bool HasDialog => !string.IsNullOrWhiteSpace(DialogId);
    }
}
