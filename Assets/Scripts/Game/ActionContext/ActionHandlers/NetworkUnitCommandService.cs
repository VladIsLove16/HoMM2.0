using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.Network;

public class NetworkUnitCommandService
{
    private readonly GameNetworkCommandGateway _gateway;

    [Inject]
    public NetworkUnitCommandService(GameNetworkCommandGateway gateway)
    {
        _gateway = gateway;
    }

    public void SendMove(UnitModel unit, IReadOnlyList<Vector2Int> gridRoute)
    {
        if (unit == null || gridRoute == null || gridRoute.Count == 0)
            return;

        if (!_gateway.TrySendMoveRequest(unit.Position.Value, new List<Vector2Int>(gridRoute)))
        {
            Debug.LogWarning("[NetworkUnitCommandService] Failed to send move request: gateway not ready or not spawned yet.");
        }
    }

    public void SendAttack(UnitModel attacker, Vector2Int targetCell)
    {
        if (attacker == null) return;
        if (!_gateway.TrySendAttackRequest(attacker.Position.Value, targetCell))
        {
            Debug.LogWarning("[NetworkUnitCommandService] Failed to send attack request: gateway not ready or not spawned yet.");
        }
    }
}


