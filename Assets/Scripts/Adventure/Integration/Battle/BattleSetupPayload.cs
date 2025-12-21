using System;

namespace Adventure.Integration.Battle
{
    /// <summary>
    /// Describes the armies and metadata required to initialise a grid battle.
    /// </summary>
    public readonly struct BattleSetupPayload
    {
        public static readonly BattleSetupPayload Empty = new BattleSetupPayload(default, SceneLoader.Scene.Adventure);

        public BattleSetupPayload(BattleArmies armies, SceneLoader.Scene returnScene)
        {
            Armies = armies;
            ReturnScene = returnScene;
        }

        public BattleArmies Armies { get; }
        public SceneLoader.Scene ReturnScene { get; }

        public bool HasData =>
            (Armies.PlayerUnits?.Count ?? 0) > 0 ||
            (Armies.EnemyUnits?.Count ?? 0) > 0;
    }
}
