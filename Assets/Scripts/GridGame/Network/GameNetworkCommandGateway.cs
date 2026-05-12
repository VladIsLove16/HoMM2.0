using Adventure.Infrastructure.State;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

[RequireComponent(typeof(NetworkObject))]
public class GameNetworkCommandGateway : NetworkBehaviour
{
    private const int DeploymentRows = 2;

    private ActionPipeline _pipeline;
    private GameModel _gameModel;
    private ITurnService _turnService;
    private SinglePlayerStartConfigurationSO _startConfiguration;

    [Inject]
    public void Init(ActionPipeline pipeline, GameModel gameModel, ITurnService turnService, SinglePlayerStartConfigurationSO startConfiguration)
    {
        _pipeline = pipeline;
        _gameModel = gameModel;
        _turnService = turnService;
        _startConfiguration = startConfiguration;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ExecuteActionServerRpc(ActionType type, ActionContext ctx, ServerRpcParams rpcParams = default)
    {
        if (!ValidateActionRequest(ctx, rpcParams.Receive.SenderClientId, out var reason))
        {
            Debug.LogWarning($"[GameNetworkCommandGateway] Rejected action {type}: {reason}");
            return;
        }

        if (!_pipeline.Execute(type, ctx))
            return;

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
        if (!ValidateStartBattleRequest(rpcParams.Receive.SenderClientId, out var reason))
        {
            Debug.LogWarning($"[GameNetworkCommandGateway] Rejected StartBattle: {reason}");
            return;
        }

        _pipeline.StartBattle();
        StartBattleClientRpc();
    }

    [ClientRpc]
    private void StartBattleClientRpc(ClientRpcParams rpcParams = default)
    {
        if (IsServer) return;

        _pipeline.StartBattle();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReturnToAdventureServerRpc(ServerRpcParams rpcParams = default)
    {
        var targetScene = BattleStateCache.GetReturnSceneOrDefault();
        if (NetworkManager == null || !NetworkManager.IsServer)
            return;

        NetworkManager.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeployUnitServerRpc(Vector2Int fromCell, Vector2Int toCell, ServerRpcParams rpcParams = default)
    {
        if (!ValidateDeploymentRequest(fromCell, toCell, rpcParams.Receive.SenderClientId, out var reason))
        {
            Debug.LogWarning($"[GameNetworkCommandGateway] Rejected deployment move {fromCell}->{toCell}: {reason}");
            return;
        }

        if (!ApplyDeploymentMove(fromCell, toCell))
            return;

        BroadcastDeployUnitClientRpc(fromCell, toCell);
    }

    [ClientRpc]
    private void BroadcastDeployUnitClientRpc(Vector2Int fromCell, Vector2Int toCell, ClientRpcParams rpcParams = default)
    {
        if (IsServer)
            return;

        ApplyDeploymentMove(fromCell, toCell);
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

    public bool RequestReturnToAdventure()
    {
        if (!CanSendRequest())
            return false;

        ReturnToAdventureServerRpc();
        return true;
    }

    public bool RequestDeployUnit(Vector2Int fromCell, Vector2Int toCell)
    {
        if (!CanSendRequest())
            return false;

        DeployUnitServerRpc(fromCell, toCell);
        return true;
    }

    private bool ValidateActionRequest(ActionContext ctx, ulong senderClientId, out string reason)
    {
        if (_turnService == null || _gameModel == null)
        {
            reason = "Gateway dependencies are not initialized.";
            return false;
        }

        if (_turnService.BattleState != BattleState.inProgress)
        {
            reason = "Battle is not in progress.";
            return false;
        }

        if (!TryResolveSenderTeam(senderClientId, out var senderTeam))
        {
            reason = $"Unable to resolve sender team for client {senderClientId}.";
            return false;
        }

        var activeUnit = _turnService.ActiveObject as UnitModel;
        if (activeUnit == null)
        {
            reason = "There is no active unit.";
            return false;
        }

        if (activeUnit.Team.Value != senderTeam)
        {
            reason = $"Client {senderClientId} tried to act for team {activeUnit.Team.Value}, but controls {senderTeam}.";
            return false;
        }

        var fromUnit = _gameModel.GetCell(ctx.FromCell)?.Unit;
        if (fromUnit == null || fromUnit != activeUnit)
        {
            reason = "Action source cell does not match the active unit.";
            return false;
        }

        reason = null;
        return true;
    }

    private bool ValidateStartBattleRequest(ulong senderClientId, out string reason)
    {
        if (_turnService == null)
        {
            reason = "Turn service is not initialized.";
            return false;
        }

        if (_turnService.BattleState != BattleState.replacement)
        {
            reason = "Deployment phase is not active.";
            return false;
        }

        if (!TryResolveSenderTeam(senderClientId, out _))
        {
            reason = $"Unable to resolve sender team for client {senderClientId}.";
            return false;
        }

        reason = null;
        return true;
    }

    private bool ValidateDeploymentRequest(Vector2Int fromCell, Vector2Int toCell, ulong senderClientId, out string reason)
    {
        if (_turnService == null || _gameModel == null)
        {
            reason = "Gateway dependencies are not initialized.";
            return false;
        }

        if (_turnService.BattleState != BattleState.replacement)
        {
            reason = "Deployment phase is not active.";
            return false;
        }

        if (!TryResolveSenderTeam(senderClientId, out var senderTeam))
        {
            reason = $"Unable to resolve sender team for client {senderClientId}.";
            return false;
        }

        if (!_gameModel.IsInBounds(fromCell) || !_gameModel.IsInBounds(toCell))
        {
            reason = "Source or target cell is out of bounds.";
            return false;
        }

        var unit = _gameModel.GetCell(fromCell)?.Unit;
        if (unit == null)
        {
            reason = "There is no unit in the source cell.";
            return false;
        }

        if (unit.Team.Value != senderTeam)
        {
            reason = $"Client {senderClientId} tried to deploy team {unit.Team.Value}, but controls {senderTeam}.";
            return false;
        }

        if (!IsWithinDeploymentZone(senderTeam, toCell))
        {
            reason = "Target cell is outside the deployment zone.";
            return false;
        }

        var targetUnit = _gameModel.GetCell(toCell)?.Unit;
        if (targetUnit != null && targetUnit != unit)
        {
            reason = "Target cell is occupied by another unit.";
            return false;
        }

        reason = null;
        return true;
    }

    private bool ApplyDeploymentMove(Vector2Int fromCell, Vector2Int toCell)
    {
        var unit = _gameModel?.GetCell(fromCell)?.Unit;
        if (unit == null)
            return false;

        if (unit.Position.Value == toCell)
            return true;

        _gameModel.MoveObject(unit, toCell);
        return true;
    }

    private bool TryResolveSenderTeam(ulong senderClientId, out Team team)
    {
        team = Team.None;

        if (_turnService == null)
            return false;

        if (NetworkManager == null || !NetworkManager.IsListening || senderClientId == NetworkManager.ServerClientId)
        {
            team = _turnService.LocalTeam;
            return team != Team.None;
        }

        var enemyTeam = _turnService.CombatUnits
            .Where(unit => unit != null)
            .Select(unit => unit.Team)
            .FirstOrDefault(unitTeam => unitTeam != Team.None && unitTeam != _turnService.LocalTeam);

        if (enemyTeam == Team.None)
            return false;

        team = enemyTeam;
        return true;
    }

    private bool IsWithinDeploymentZone(Team team, Vector2Int cell)
    {
        var cells = _gameModel?.GetAllCells();
        if (cells == null)
            return false;

        var height = cells.GetLength(1);
        if (height <= 0)
            return false;

        var rows = Mathf.Clamp(DeploymentRows, 1, height);
        return !IsBottomTeam(team)
            ? cell.y >= height - rows
            : cell.y < rows;
    }

    private bool IsBottomTeam(Team team)
    {
        var bottomTeam = _startConfiguration?.BattlefieldBottomTeam ?? _turnService.LocalTeam;
        return team == bottomTeam;
    }
}
