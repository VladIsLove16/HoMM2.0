using UnityEngine;

public class NetworkGameCommandExecutor : IGameCommandExecutor
{
    private readonly GameNetworkCommandGateway _gateway;

    public NetworkGameCommandExecutor(GameNetworkCommandGateway gateway)
    {
        _gateway = gateway;
    }

    public void StartBattle()
    {
        if (!_gateway.RequestStartBattle())
        {
            Debug.LogWarning("[NetworkGameCommandExecutor] Unable to send StartBattle request.");
        }
    }

    public bool Execute(ActionType type, ActionContext ctx)
    {
        Debug.Log("Execute " + type);
        if (!_gateway.RequestExecuteAction(type, ctx))
        {
            Debug.LogWarning($"[NetworkGameCommandExecutor] Unable to send action {type}.");
            return false;
        }
        return true;
    }

    public bool TryDeployUnit(Vector2Int fromCell, Vector2Int toCell)
    {
        if (!_gateway.RequestDeployUnit(fromCell, toCell))
        {
            Debug.LogWarning($"[NetworkGameCommandExecutor] Unable to send deployment move {fromCell} -> {toCell}.");
            return false;
        }

        return true;
    }
}
