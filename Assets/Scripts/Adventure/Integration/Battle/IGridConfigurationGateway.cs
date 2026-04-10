namespace Adventure.Integration.Battle
{
    public interface IGridConfigurationGateway
    {
        SceneLoader.Scene BattleScene { get; }
        Team PlayerTeam { get; }
        void PrepareBattle(BattleSetupPayload payload, ArmyFormationResolver formationResolver);
    }
}
