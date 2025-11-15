using Unity.Netcode;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(NetworkObject))]
public class GameNetworkCommandGateway : NetworkBehaviour
{
    private ActionPipeline _pipeline;

    [Inject]
    public void Init(ActionPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ExecuteActionServerRpc(ActionType type, ActionContext ctx, ServerRpcParams rpcParams = default)
    {
         _pipeline.Execute(type, ctx);
        BroadcastActionClientRpc(type, ctx);
    }

    [ClientRpc]
    private void BroadcastActionClientRpc(ActionType type, ActionContext ctx, ClientRpcParams rpcParams = default)
    {
        if (IsServer) return;
        _pipeline.Execute(type, ctx);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartBattleServerRpc(ServerRpcParams rpcParams = default)
    {
        _pipeline.StartBattle();
        StartBattleClientRpc();
    }

    [ClientRpc]
    private void StartBattleClientRpc(ClientRpcParams rpcParams = default)
    {
        if (IsServer) return;

        _pipeline.StartBattle();
    }

    private bool CanSendRequest()
    {
        if (!isActiveAndEnabled)
        {
            Debug.LogWarning("[GameNetworkCommandGateway] Gateway is disabled.");
            return false;
        }

        var netObj = GetComponent<NetworkObject>();
        if (netObj == null || !netObj.IsSpawned)
        {
            Debug.LogWarning("[GameNetworkCommandGateway] NetworkObject is not spawned yet.");
            return false;
        }

        return true;
    }

    public bool RequestExecuteAction(ActionType type, ActionContext ctx)
    {
        if (!CanSendRequest())
            return false;

        ExecuteActionServerRpc(type, ctx);
        return true;
    }

    public bool RequestStartBattle()
    {
        if (!CanSendRequest())
            return false;

        StartBattleServerRpc();
        return true;
    }
}
