using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ClientGameRpcService
{
    private readonly GameModel _gameModel;

    [Inject]
    public ClientGameRpcService(GameModel gameModel)
    {
        _gameModel = gameModel;
    }

    public void ApplyClientAttack(Vector2Int attackerCell, Vector2Int targetCell)
    {
        var attacker = _gameModel.GetCell(attackerCell).Unit;
        var target = _gameModel.GetCell(targetCell).Unit as IDamagable;
        if (attacker == null || target == null) return;
        attacker.SendDamage(new AttackContext(target));
    }

    public void ApplyClientMove(Vector2Int startCell, Vector2Int[] gridRoute)
    {
        var unit = _gameModel.GetCell(startCell).Unit;
        if (unit == null || gridRoute == null || gridRoute.Length == 0) return;
        _gameModel.MoveObject(unit, new List<Vector2Int>(gridRoute));
    }

    public void ApplySpawn(UnitSpawnParams unitSpawnParams)
    {
        Debug.Log("Start Spawning unit with " + unitSpawnParams.ToString());
        _gameModel.SpawnUnit(unitSpawnParams);
    }
}




