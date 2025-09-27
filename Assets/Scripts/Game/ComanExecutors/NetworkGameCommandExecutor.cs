public class NetworkGameCommandExecutor : IGameCommandExecutor
{
    private readonly GameNetworkCommandGateway _gateway;

    public NetworkGameCommandExecutor(GameNetworkCommandGateway gateway)
    {
        _gateway = gateway;
    }

    public void StartBattle()
    {
        _gateway.RequestStartBattle();
    }

    public void Execute(ActionType type, ActionContext ctx)
    {
        _gateway.RequestExecuteAction(type, ctx);
    }
}
