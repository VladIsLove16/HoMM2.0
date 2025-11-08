namespace Adventure.Integration.Battle
{
    public interface IGridConfigurationGateway
    {
        Loader.Scene BattleScene { get; }
        void PrepareBattle(BattleSetupPayload payload, ArmyFormationResolver formationResolver);
    }
}
