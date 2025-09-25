using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.Netcode;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(NetworkObject))]
public class GameNetworkCommandGateway : NetworkBehaviour
{
    public enum ClientStage
    {
        Disconnected,
        WaitingInitGrid,
        GridReady,
        SpawningUnits,
        UnitsReady,
        BattleStarted
    }
    public enum ServerStage
    {
        Idle,
        WaitingClientsGridReady,
        SendingUnits,
        WaitingClientsUnitsReady,
        BattleStarted,
        WaitingClientsSceneReady
    }
    // Синхронизация состояний удалена — только командная модель

    [Inject] private GameModel _gameModel;
    [Inject] private GameViewModel _gameViewModel;
    [Inject] private TurnSystem _turnSystem;
    [Inject] private GameController _gameController;
    [Inject] private SceneTransitionDataService _sceneTransitionDataService;
    [Inject] private ServerGameRpcService _serverService;
    [Inject] private ClientGameRpcService _clientService;
    public event System.Action<ClientStage> ClientStageChanged;
    public event System.Action<ServerStage, int, int> ServerStageChanged; // (stage, readyCount, total)
    private ClientStage _clientStage;
    private ServerStage _serverStage;
    private readonly HashSet<UnitModel> _dirtyUnits = new HashSet<UnitModel>();
    private readonly HashSet<ulong> _clientsGridReady = new HashSet<ulong>();
    private readonly HashSet<ulong> _clientsUnitsReady = new HashSet<ulong>();
    private readonly HashSet<ulong> _clientsSceneReady = new HashSet<ulong>();
    private int _pendingWidth;
    private int _pendingHeight;
    private SpawnUnitMsg[] _pendingUnits;
    private int _stateVersion;
    private bool battleSettuped = false;
    private bool battleSettupSended = false;

    private void OnEnable()
    {
        // Fail-safe: если DI не проставил сервисы, создаём их из _gameModel
        if (_serverService == null && _gameModel != null)
        {
            Debug.LogAssertion(" DI не проставил _serverService, создаём их из _gameModel");
            _serverService = new ServerGameRpcService(_gameModel);
        }
        if (_clientService == null && _gameModel != null)
        {
            Debug.LogAssertion(" DI не проставил _clientService, создаём их из _gameModel");
            _clientService = new ClientGameRpcService(_gameModel);
        }

        _serverStage = ServerStage.Idle;
        _clientStage = NetworkManager != null && NetworkManager.IsServer ? ClientStage.BattleStarted : ClientStage.WaitingInitGrid;
        ClientStageChanged?.Invoke(_clientStage);
        ServerStageChanged += OnServerStageChanged;
        ClientStageChanged += OnClientStageChanged;
        // Передаём флаг роли в контроллер
        bool isHost = NetworkManager != null && NetworkManager.IsServer;
        _gameController.SetIsHostFlag(isHost);
        Debug.Log(" this is " + ( isHost ? " " : "not") + " host");
    }
    private void OnClientStageChanged(ClientStage stage)
    {
        Debug.Log("new client stage : " + stage);
    }

    private void OnServerStageChanged(ServerStage stage, int arg2, int arg3)
    {
        Debug.Log("new server stage : " + stage);
    }

    // ==== SPAWN SYNC (только при начальном сетапе через батч) ====
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

    [ServerRpc(RequireOwnership = false)]
    private void SpawnUnitServerRpc(int x, int y, UnitType unitType, int amount, bool isPlayer, ServerRpcParams rpcParams = default)
    {
        _serverService.ApplyServerSpawn(x, y, unitType, amount, isPlayer);
        SpawnAcknowledgeClientRpc(x, y, unitType, amount, isPlayer);
    }

    [ClientRpc]
    private void SpawnAcknowledgeClientRpc(int x, int y, UnitType unitType, int amount, bool isPlayer)
    {
        var unitSpawnParams = new UnitSpawnParams(x, y, unitType, amount, isPlayer);
        _clientService.ApplySpawn(unitSpawnParams);
    }

    [ClientRpc]
    public void StartBattleClientRpc()
    {
        _gameController.RunBattle();
    }

    private void SetupBattleForClients()
    {
        if (battleSettuped)
            return;
        if (!IsServer || NetworkManager == null) return;
        List<SpawnUnitMsg> list = GetLobbyConfiguration();
        _pendingUnits = list.ToArray();
        _pendingWidth = _sceneTransitionDataService.Width;
        _pendingHeight = _sceneTransitionDataService.Height;

        _clientsGridReady.Clear();
        _clientsUnitsReady.Clear();


        Debug.Log("battle settuped");
        battleSettuped = true;
    }

    private List<SpawnUnitMsg> GetLobbyConfiguration()
    {
        var gce = _sceneTransitionDataService.GetSelectedConfiguration();
        var list = new List<SpawnUnitMsg>();
        foreach (var u in gce.contents)
        {
            list.Add(new SpawnUnitMsg
            {
                X = u.X,
                Y = u.Y,
                Amount = u.Amount,
                IsPlayer = u.isPlayer,
                UnitType = u.unitType
            });
        }
        return list;
    }
    private List<SpawnUnitMsg> GetCurrentConfiguration()
    {
        var units = _gameModel.GetUnits();
        var list = new List<SpawnUnitMsg>(units.Count);
        foreach (var u in units)
        {
            list.Add(new SpawnUnitMsg
            {
                X = u.Position.Value.x,
                Y = u.Position.Value.y,
                Amount = u.Amount.Value,
                IsPlayer = u.IsBlueTeam.Value,
                UnitType = u.UnitType.Value
            });
        }

        return list;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ClientSceneLoadedServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("ClientSceneLoadedServerRpc");
        var sender = rpcParams.Receive.SenderClientId;
        _clientsSceneReady.Add(sender);

        //создаём данные для отправки клиентам.
        SetupBattleForClients();
        // Хост/клиент id не требуются для логики ходов
        //отправляем всем клиентам данные об игре.
        SendBattleSetupClientRpc(_pendingWidth, _pendingHeight, _pendingUnits);
    }

    public void TrySendBattleSetup()
    {
        //если куратина уже идет, ничего не делаем
        if (_serverStage == ServerStage.WaitingClientsSceneReady)
            return;
        //создаём данные для отправки клиентам.
        SetupBattleForClients();
        if (!IsServer || NetworkManager == null) return;
        //ожидаем готовности сцены у всех клиентов
        StartCoroutine(WaitingClientsSceneReadyCoroutine());
    }
    private IEnumerator WaitingClientsSceneReadyCoroutine()
    {
        //колво клиентов завычетом хоста
        var total = NetworkManager.ConnectedClientsIds.Count - 1;
        _serverStage = ServerStage.WaitingClientsSceneReady;
        ServerStageChanged?.Invoke(_serverStage, _clientsGridReady.Count, total);
        while (true)
        {
            Debug.Log("WaitingClientsSceneReady ");
            Debug.Log("_clientsSceneReady " + _clientsSceneReady.Count + " NetworkManager.ConnectedClientsIds.Count " + NetworkManager.ConnectedClientsIds.Count + " _pendingUnits + " + _pendingUnits.Length); ;
            if (_clientsSceneReady.Count >= total && _pendingUnits != null)
            {
                _serverStage = ServerStage.SendingUnits;
                ServerStageChanged?.Invoke(_serverStage, _clientsGridReady.Count, total);
                SendBattleSetupClientRpc(_pendingWidth, _pendingHeight, _pendingUnits);
                _serverStage = ServerStage.WaitingClientsUnitsReady;
                ServerStageChanged?.Invoke(_serverStage, _clientsUnitsReady.Count, total);
                break;
            }
            yield return new WaitForSeconds(1f);
        }
    }
    [ClientRpc]
    private void SendBattleSetupClientRpc(int width, int height, SpawnUnitMsg[] units)
    {
        // Хост не должен применять клиентский сетап
        if (IsServer) return;
        if (battleSettupSended)
            return;
        battleSettupSended = true;
        // 1) сетка
        _clientStage = ClientStage.WaitingInitGrid;
        ClientStageChanged?.Invoke(_clientStage);
        _gameController.Setup(width, height);
        _clientStage = ClientStage.GridReady;
        ClientStageChanged?.Invoke(_clientStage);
        //ClientGridReadyServerRpc();

        // 2) юниты
        _clientStage = ClientStage.SpawningUnits;
        ClientStageChanged?.Invoke(_clientStage);
        if (units != null)
        {
            foreach (var s in units)
            {
                var p = new UnitSpawnParams(s.X, s.Y, s.UnitType, s.Amount, s.IsPlayer);
                _clientService.ApplySpawn(p);
            }
        }
        _gameController.InitTurnSystem();
        _clientStage = ClientStage.UnitsReady;
        ClientStageChanged?.Invoke(_clientStage);
        ClientUnitsReadyServerRpc();
    }

    //[ServerRpc]
    //private void ClientGridReadyServerRpc(ServerRpcParams rpcParams = default)
    //{
    //    var sender = rpcParams.Receive.SenderClientId;
    //    _clientsGridReady.Add(sender);
    //    ServerStageChanged?.Invoke(_serverStage, _clientsGridReady.Count, NetworkManager.ConnectedClientsIds.Count);
    //    if (_clientsGridReady.Count >= NetworkManager.ConnectedClientsIds.Count)
    //    {
    //        _serverStage = ServerStage.SendingUnits;
    //        ServerStageChanged?.Invoke(_serverStage, _clientsGridReady.Count, NetworkManager.ConnectedClientsIds.Count);
    //        SendUnitsBatchToClients();
    //    }
    //}

    public struct SpawnUnitMsg : Unity.Netcode.INetworkSerializable
    {
        public int X;
        public int Y;
        public int Amount;
        public bool IsPlayer;
        public UnitType UnitType;
        public void NetworkSerialize<T>(Unity.Netcode.BufferSerializer<T> serializer) where T : Unity.Netcode.IReaderWriter
        {
            serializer.SerializeValue(ref X);
            serializer.SerializeValue(ref Y);
            serializer.SerializeValue(ref Amount);
            serializer.SerializeValue(ref IsPlayer);
            var t = (int)UnitType;
            serializer.SerializeValue(ref t);
            UnitType = (UnitType)t;
        }
    }

    //private void SendUnitsBatchToClients()
    //{
    //    var units = _gameModel.GetUnits();
    //    var list = new List<SpawnUnitMsg>(units.Count);
    //    foreach (var u in units)
    //    {
    //        list.Add(new SpawnUnitMsg
    //        {
    //            X = u.Position.Value.x,
    //            Y = u.Position.Value.y,
    //            Amount = u.Amount.Value,
    //            IsPlayer = u.IsBlueTeam.Value,
    //            UnitType = u.UnitType.Value
    //        });
    //    }
    //    SendUnitsBatchClientRpc(list.ToArray());
    //    _serverStage = ServerStage.WaitingClientsUnitsReady;
    //    ServerStageChanged?.Invoke(_serverStage, _clientsUnitsReady.Count, NetworkManager.ConnectedClientsIds.Count);
    //}

    [ClientRpc]
    private void SendUnitsBatchClientRpc(SpawnUnitMsg[] units)
    {
        _clientStage = ClientStage.SpawningUnits;
        ClientStageChanged?.Invoke(_clientStage);
        if (units != null)
        {
            foreach (var s in units)
            {
                var p = new UnitSpawnParams(s.X, s.Y, s.UnitType, s.Amount, s.IsPlayer);
                _gameModel.SpawnUnit(p);
            }
        }
        // Инициализируем локальный TurnSystem на клиенте
        _gameController.InitTurnSystem();
        _clientStage = ClientStage.UnitsReady;
        ClientStageChanged?.Invoke(_clientStage);
        ClientUnitsReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClientUnitsReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        var sender = rpcParams.Receive.SenderClientId;
        _clientsUnitsReady.Add(sender);
        
        //ServerStageChanged?.Invoke(_serverStage, _clientsUnitsReady.Count, NetworkManager.ConnectedClientsIds.Count);
        //if (_clientsUnitsReady.Count >= NetworkManager.ConnectedClientsIds.Count)
        //{
        //    _serverStage = ServerStage.BattleStarted;
        //    ServerStageChanged?.Invoke(_serverStage, _clientsUnitsReady.Count, NetworkManager.ConnectedClientsIds.Count);
        //    StartBattleForAll();
        //}
    }

    public void StartBattleForAll()
    {
        // Старт боя на сервере/хосте
        // И на клиентах
        StartBattleClientRpc();
        // Ход определяется локально по цвету активного юнита (синий == хост)
    }

    public OperationResult TrySendEndTurnRequest()
    {
        if (NetworkManager == null) return new OperationResult(false, "NetworkManager == null");
        EndTurnServerRpc();
        return new OperationResult(true);

    }
    [ServerRpc(RequireOwnership = false)]
    private void EndTurnServerRpc()
    {
        _turnSystem.EndTurn();
        EndTurnClientRpc();
    }
    [ClientRpc]
    private void EndTurnClientRpc()
    {
        // На хосте ход уже завершён на сервере
        if (IsServer) return;
        _turnSystem.EndTurn();
    }
    [ServerRpc(RequireOwnership = false)]
    public void TrySendExecutionServerRpc(ActionType type, ActionContext actionContext)
    {
        if (_gameViewModel.CanExecute(type, actionContext))
        {
            _gameViewModel.Execute(type, actionContext);
            ExecuteAcknowledgeClientRpc(type, actionContext);
        }
    }
    [ClientRpc(RequireOwnership = false)]
    public void ExecuteAcknowledgeClientRpc(ActionType type, ActionContext actionContext)
    {
        _gameViewModel.Execute(type, actionContext);
    }
}
