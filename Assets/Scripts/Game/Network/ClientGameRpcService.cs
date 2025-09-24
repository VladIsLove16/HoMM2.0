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

    public void ApplySpawn(UnitSpawnParams unitSpawnParams)
    {
        Debug.Log("Start Spawning unit with " + unitSpawnParams.ToString());
        _gameModel.SpawnUnit(unitSpawnParams);
    }
}




