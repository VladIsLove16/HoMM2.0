namespace Adventure.Integration.Battle
{
    public interface IGridConfigurationGateway
    {
        SceneLoader.Scene BattleScene { get; }
        void PrepareBattle(BattleSetupPayload payload, ArmyFormationResolver formationResolver);
    }
}
