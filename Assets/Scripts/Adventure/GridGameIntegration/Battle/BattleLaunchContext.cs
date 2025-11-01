namespace Adventure.Integration.Battle
{
    public sealed class BattleLaunchContext
    {
        public BattleLaunchContext(
            ArmyLineupSO enemyArmy,
            string dialogId = null,
            string victoryNodeId = null,
            string defeatNodeId = null)
        {
            EnemyArmy = enemyArmy;
            DialogId = dialogId;
            VictoryNodeId = victoryNodeId;
            DefeatNodeId = defeatNodeId;
        }

        public ArmyLineupSO EnemyArmy { get; }
        public string DialogId { get; }
        public string VictoryNodeId { get; }
        public string DefeatNodeId { get; }

        public bool HasDialogContext => !string.IsNullOrEmpty(DialogId);
    }
}
