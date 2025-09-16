using JetBrains.Annotations;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.Netcode;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(NetworkObject))]
public class GameNetworkCommandGateway : NetworkBehaviour
{
    public struct UnitState : Unity.Netcode.INetworkSerializable
    {
        public int CellX;
        public int CellY;
        public bool IsBlueTeam;
        public int Amount;
        public int Health;
        public int MaxHealth;
        public UnitType UnitType;
        public int Version;
        public void NetworkSerialize<T>(Unity.Netcode.BufferSerializer<T> serializer) where T : Unity.Netcode.IReaderWriter
        {
            serializer.SerializeValue(ref CellX);
            serializer.SerializeValue(ref CellY);
            serializer.SerializeValue(ref IsBlueTeam);
            serializer.SerializeValue(ref Amount);
            serializer.SerializeValue(ref Health);
            serializer.SerializeValue(ref MaxHealth);
            var unitTypeInt = (int)UnitType;
            serializer.SerializeValue(ref unitTypeInt);
            UnitType = (UnitType)unitTypeInt;
            serializer.SerializeValue(ref Version);
        }
    }

    [Inject] private GameModel _gameModel;
    [Inject] private TurnSystem _turnSystem;
    [Inject] private GameController _gameController;
    [Inject] private ServerGameRpcService _serverService;
    [Inject] private ClientGameRpcService _clientService;
    private readonly HashSet<UnitModel> _dirtyUnits = new HashSet<UnitModel>();
    private int _stateVersion;
    private readonly Queue<UnitSpawnParams> _pendingSpawns = new Queue<UnitSpawnParams>();

    private void FlushPendingSpawns()
    {
        if (_gameModel == null) return;
        while (_pendingSpawns.Count > 0 && _gameModel.IsGridInitialized)
        {
            var spawnParams = _pendingSpawns.Dequeue();
            SafeSpawn(spawnParams);
        }
    }

    private void SafeSpawn(UnitSpawnParams spawnParams)
    {
        if (_gameModel == null)
        {
            _pendingSpawns.Enqueue(spawnParams);
            return;
        }
        if (!_gameModel.IsGridInitialized)
        {
            _pendingSpawns.Enqueue(spawnParams);
            return;
        }
        var pos = new Vector2Int(spawnParams.X, spawnParams.Y);
        if (!_gameModel.IsInBounds(pos))
        {
            Debug.LogWarning($"[GameNetworkCommandGateway] Spawn out of bounds {pos} for {spawnParams.UnitType}");
            return;
        }
        _gameModel.SpawnUnit(spawnParams);
    }

    private void OnEnable()
    {
        // Fail-safe: если DI не проставил сервисы, создаём их из _gameModel
        if (_serverService == null && _gameModel != null)
            _serverService = new ServerGameRpcService(_gameModel);
        if (_clientService == null && _gameModel != null)
            _clientService = new ClientGameRpcService(_gameModel);

        if (_serverService != null)
            _serverService.SetMarkDirtyCallback(MarkDirty);

        // Подписка на инициализацию сетки для отложенных спавнов
        if (_gameModel != null)
        {
            _gameModel.GridInitialized += _ => FlushPendingSpawns();
            if (_gameModel.IsGridInitialized) FlushPendingSpawns();
        }
    }

    private void OnDisable()
    {
        if (_gameModel != null)
        {
            _gameModel.GridInitialized -= _ => FlushPendingSpawns();
        }
    }

    public bool TrySendMoveRequest(Vector2Int startCell, List<Vector2Int> gridRoute)
    {
        if (gridRoute == null || gridRoute.Count == 0) return false;
        if (NetworkManager.Singleton == null) return false;

        // На хосте можно применить напрямую без RPC
        if (NetworkManager.Singleton.IsServer)
        {
            _serverService.ApplyServerMove(startCell, gridRoute);
            MoveAcknowledgeClientRpc(startCell, gridRoute.ToArray());
            return true;
        }

        if (!IsSpawned)
        {
            Debug.LogWarning("[GameNetworkCommandGateway] NetworkObject is not spawned yet. Skipping send.");
            return false;
        }

        MoveRequestServerRpc(startCell, gridRoute.ToArray());
        return true;
    }

    [ServerRpc]
    private void MoveRequestServerRpc(Vector2Int startCell, Vector2Int[] gridRoute, ServerRpcParams rpcParams = default)
    {
        if (gridRoute == null || gridRoute.Length == 0) return;
        _serverService.ApplyServerMove(startCell, new List<Vector2Int>(gridRoute));
        MoveAcknowledgeClientRpc(startCell, gridRoute);
    }

    [ClientRpc]
    private void MoveAcknowledgeClientRpc(Vector2Int startCell, Vector2Int[] gridRoute)
    {
        var unit = _gameModel.GetCell(startCell).Unit;
        if (unit == null || gridRoute == null || gridRoute.Length == 0) return;
        _gameModel.MoveObject(unit, new List<Vector2Int>(gridRoute));
    }


    public bool TrySendAttackRequest(Vector2Int attackerCell, Vector2Int targetCell)
    {
        if (NetworkManager.Singleton == null) return false;

        if (NetworkManager.Singleton.IsServer)
        {
            _serverService.ApplyServerAttack(attackerCell, targetCell);
            AttackAcknowledgeClientRpc(attackerCell, targetCell);
            return true;
        }

        if (!IsSpawned)
        {
            Debug.LogWarning("[GameNetworkCommandGateway] NetworkObject is not spawned yet. Skipping attack send.");
            return false;
        }

        AttackRequestServerRpc(attackerCell, targetCell);
        return true;
    }

    [ServerRpc]
    private void AttackRequestServerRpc(Vector2Int attackerCell, Vector2Int targetCell, ServerRpcParams rpcParams = default)
    {
        _serverService.ApplyServerAttack(attackerCell, targetCell);
        AttackAcknowledgeClientRpc(attackerCell, targetCell);
    }

    [ClientRpc]
    private void AttackAcknowledgeClientRpc(Vector2Int attackerCell, Vector2Int targetCell)
    {
        _clientService.ApplyClientAttack(attackerCell, targetCell);
    }


    // ==== SPAWN SYNC ====
    public bool TrySendSpawnUnit(int x, int y, UnitType unitType, int amount, bool isPlayer)
    {
        
        if (NetworkManager.Singleton == null)
        {
            throw new NetworkConfigurationException("NetworkManager.Singleton == null");
            //_gameModel.SpawnUnit(new UnitSpawnParams(x, y, unitType, amount, isPlayer));
        }

        // Нельзя слать RPC до спауна этого NetworkBehaviour
        if (!IsSpawned)
        {
            Debug.LogWarning("[GameNetworkCommandGateway] Not spawned yet, skip TrySendSpawnUnit this frame.");
            return false;
        }

        if (NetworkManager.Singleton.IsServer)
        {
            _serverService.ApplyServerSpawn(x, y, unitType, amount, isPlayer);
            SpawnAcknowledgeClientRpc(x, y, unitType, amount, isPlayer);
            return true;
        }

        if (!IsSpawned)
        {
            Debug.LogWarning("[GameNetworkCommandGateway] Not spawned, skip spawn send.");
            return false;
        }

        SpawnUnitServerRpc(x, y, unitType, amount, isPlayer);
        return true;
    }

    [ServerRpc]
    private void SpawnUnitServerRpc(int x, int y, UnitType unitType, int amount, bool isPlayer, ServerRpcParams rpcParams = default)
    {
        _serverService.ApplyServerSpawn(x, y, unitType, amount, isPlayer);
        SpawnAcknowledgeClientRpc(x, y, unitType, amount, isPlayer);
    }

    [ClientRpc]
    private void SpawnAcknowledgeClientRpc(int x, int y, UnitType unitType, int amount, bool isPlayer)
    {
        var unitSpawnParams = new UnitSpawnParams(x, y, unitType, amount, isPlayer);
        Debug.Log("Start Spawning unit with " + unitSpawnParams.ToString());
        SafeSpawn(unitSpawnParams);
    }

    

    // ==== REMOVE SYNC ====
    public bool TrySendRemoveUnit(Vector2Int cell)
    {
        if (NetworkManager.Singleton == null)
        {
            _serverService.ApplyServerRemove(cell);
            return true;
        }
        if (NetworkManager.Singleton.IsServer)
        {
            _serverService.ApplyServerRemove(cell);
            RemoveAcknowledgeClientRpc(cell);
            return true;
        }
        if (!IsSpawned)
        {
            Debug.LogWarning("[GameNetworkCommandGateway] Not spawned, skip remove send.");
            return false;
        }
        RemoveUnitServerRpc(cell);
        return true;
    }

    [ServerRpc]
    private void RemoveUnitServerRpc(Vector2Int cell, ServerRpcParams rpcParams = default)
    {
        _serverService.ApplyServerRemove(cell);
        RemoveAcknowledgeClientRpc(cell);
    }

    [ClientRpc]
    private void RemoveAcknowledgeClientRpc(Vector2Int cell)
    {
        ApplyClientRemove(cell);
    }

    private void ApplyClientRemove(Vector2Int cell)
    {
        var unit = _gameModel.GetCell(cell).Unit;
        if (unit == null) return;
        unit.Died?.Invoke();
    }

    private void MarkDirty(UnitModel unit)
    {
        if (unit == null) return;
        _dirtyUnits.Add(unit);
    }

    [ClientRpc]
    public void InitClientTurnSystemClientRpc()
    {
        foreach (var unit in _gameModel.GetUnits())
            _turnSystem.AddCombatUnit(unit);
    }

    [ClientRpc]
    public void StartBattleClientRpc()
    {
        _gameController.RunBattle();
    }
    private UnitState ToState(UnitModel unit)
    {
        return new UnitState
        {
            CellX = unit.Position.Value.x,
            CellY = unit.Position.Value.y,
            IsBlueTeam = unit.IsBlueTeam.Value,
            Amount = unit.Amount.Value,
            Health = unit.ModifiedStats.Health,
            MaxHealth = unit.ModifiedStats.MaxHealth,
            UnitType = unit.UnitType.Value,
            Version = ++_stateVersion
        };
    }

    private void LateUpdate()
    {
        if (!IsServer || _dirtyUnits.Count == 0) return;
        var list = new List<UnitState>(_dirtyUnits.Count);
        foreach (var u in _dirtyUnits)
            list.Add(ToState(u));
        _dirtyUnits.Clear();
        SyncUnitsStateClientRpc(list.ToArray());
    }

    [ClientRpc]
    private void SyncUnitsStateClientRpc(UnitState[] states)
    {
        if (states == null || states.Length == 0) return;
        foreach (var s in states)
        {
            _clientService.ApplyUnitState(s);
            // позиция уже синхронизируется Move, но при необходимости можно форсировать
        }
    }
}


