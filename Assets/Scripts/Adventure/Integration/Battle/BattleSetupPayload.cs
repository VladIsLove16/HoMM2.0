using System;

namespace Adventure.Integration.Battle
{
    /// <summary>
    /// Describes the armies and metadata required to initialise a grid battle.
    /// </summary>
    public readonly struct BattleSetupPayload
    {
        public static readonly BattleSetupPayload Empty = new BattleSetupPayload(default, SceneLoader.Scene.Adventure, Team.Blue, Team.Blue);

        public BattleSetupPayload(BattleArmies armies, SceneLoader.Scene returnScene, Team localTeam, Team battlefieldBottomTeam)
        {
            Armies = armies;
            ReturnScene = returnScene;
            LocalTeam = localTeam;
            BattlefieldBottomTeam = battlefieldBottomTeam;
        }

        public BattleArmies Armies { get; }
        public SceneLoader.Scene ReturnScene { get; }
        public Team LocalTeam { get; }
        public Team BattlefieldBottomTeam { get; }

        public bool HasData =>
            (Armies.PlayerUnits?.Count ?? 0) > 0 ||
            (Armies.EnemyUnits?.Count ?? 0) > 0;
    }
}
