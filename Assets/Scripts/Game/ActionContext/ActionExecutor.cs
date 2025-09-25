using UnityEngine;
public class LocalActionExecutor : IActionExecutor
{
    public LocalActionExecutor()
    {

    }

    public void Execute(IActionHandler actionHandler,ActionContext actionContext)
    {
        actionHandler.Execute(actionContext);
    }
}
/// <summary>
/// Сетевой исполнитель команд для мультиплеера
/// </summary>
public class NetworkActionExecutor : IActionExecutor
{
    private readonly GameNetworkCommandGateway _networkGateway;

    public NetworkActionExecutor(GameNetworkCommandGateway networkGateway)
    {
        _networkGateway = networkGateway;
    }

    public void Execute(IActionHandler actionHandler, ActionContext actionContext)
    {
        _networkGateway.TrySendExecutionServerRpc(actionHandler.ActionType,actionContext);
    }
}
