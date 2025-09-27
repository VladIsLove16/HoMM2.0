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

    public void RequestExecuteAction(ActionType type, ActionContext ctx) => ExecuteActionServerRpc(type, ctx);
    public void RequestStartBattle() => StartBattleServerRpc();
}
