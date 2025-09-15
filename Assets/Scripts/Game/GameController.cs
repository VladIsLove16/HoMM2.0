using System;
using UnityEngine;
using Zenject;
using NaughtyAttributes;
using Unity.Netcode;

/// <summary>
/// Основной контроллер игры
/// Управляет инициализацией игрового процесса и созданием юнитов
/// </summary>
public class GameController : NetworkBehaviour, IInitializable
{
    [Header("Grid Settings")]
    [SerializeField] private int Height = 8;
    [SerializeField] private int Width = 8;
    
    [Header("Fallback Configuration")]
    [SerializeField] private GridContentEntrySO gridContentEntrySO;

    private GameModel _gameModel;
    private TurnSystem _combatSystem;
    private IGameConfigurationProvider _configurationProvider;

    private Vector2Int? selectedCellCoords;
    private bool isCellSelected;
    [SerializeField] private GameNetworkCommandGateway _gateway;

    [Inject]
    public void Construct(
        GameViewModel viewModel, 
        GridView view, 
        GameModel model, 
        TurnSystem combatSystem,
        IGameConfigurationProvider configurationProvider)
    {
        _gameModel = model;
        _combatSystem = combatSystem;
        _configurationProvider = configurationProvider;
    }
    
    public void Initialize()
    {
        Debug.Log("[GameController] Initialized with configuration provider");
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("[GameController] OnNetworkSpawn called");
        Debug.Log($"[GameController] NetworkObjectId: {NetworkObjectId}, IsHost: {IsHost}, IsClient: {IsClient}");
        
        Setup(Width, Height);
        
        // Всегда готовим систему ходов до возможных спавнов, чтобы подписки были активны
        InitCombatSystem();
        
        if (IsHost)
        {
            Debug.Log("[GameController] IsHost - creating grid content");
            // Дождаться спауна сетевого шлюза и только затем слать RPC
            StartCoroutine(WaitForGatewayAndStart());
        }
        else
        {
            Debug.Log("[GameController] IsClient - waiting for host to create units");
        }
    }
    
    private void Start()
    {
        Debug.Log("[GameController] Start called");
        
        // Локальный режим (без сети) — только если нет NetworkManager вообще
        if (NetworkManager.Singleton == null)
        {
            Debug.Log("[GameController] No NetworkManager - running in local mode");
            Setup(Width, Height);
            InitCombatSystem();
            CreateGridContentFromConfiguration();
            RunBattle();
        }
        else
        {
            // Сетевой режим: инициализация производится в OnNetworkSpawn
            Debug.Log("[GameController] Network mode detected, waiting for OnNetworkSpawn");
        }
    }

    private void InitCombatSystem()
    {
        // Синхронизируем текущих юнитов
        _combatSystem.ClearUnits();
        foreach (var unit in _gameModel.GetUnits())
        {
            _combatSystem.AddCombatUnit(unit);
        }

        _gameModel.UnitSpawned += OnGameModel_UnitSpawned;
        _gameModel.UnitDied += OnGameModel_UnitDied;
    }

    private void OnGameModel_UnitDied(ContentDiedParams @params)
    {
        if (@params?.UnitModel != null)
        {
            _combatSystem.RemoveCombatUnit(@params.UnitModel);
        }
    }

    private void OnGameModel_UnitSpawned(UnitModelCreatedParams @params)
    {
        if (@params?.UnitModel != null)
        {
            _combatSystem.AddCombatUnit(@params.UnitModel);
        }
    }

    [Button]
    public void Setup()
    {
        Setup(Width, Height);
    }

    [Button]
    public void RunBattle()
    {
        if(NetworkManager.Singleton== null ) 
        {
            _combatSystem.RunBattle();

        }
        else if(NetworkManager.Singleton.ConnectedClientsIds.Count < 2 && _configurationProvider.AcceptStartingBattleWithoutClients )
        {
            _combatSystem.RunBattle();
        }
        else if(NetworkManager.Singleton.ConnectedClientsIds.Count >= 2 )
        {
            _combatSystem.RunBattle();
        }
        else
        {
            Debug.Log("wait for clients connect battle");
            return;
        }
            Debug.Log("Run battle");
    }

    [Button]
    public void CreateGridContent()
    {
        CreateGridContent(gridContentEntrySO);
    }

    private void CreateGridContentFromConfiguration()
    {
        Debug.Log("[GameController] CreateGridContentFromConfiguration called");
        
        if (_configurationProvider == null)
        {
            Debug.LogError("[GameController] Configuration provider is null!");
            CreateFallbackGridContent();
            return;
        }
        
        Debug.Log($"[GameController] Configuration provider found: {_configurationProvider.GetType().Name}");
        
        // Получаем выбранную конфигурацию через DI
        var selectedConfig = _configurationProvider.GetSelectedConfiguration();
        if (selectedConfig != null)
        {
            Debug.Log($"[GameController] Creating grid content from configuration: {selectedConfig.name}");
            CreateGridContent(selectedConfig);
        }
        else
        {
            Debug.LogWarning("[GameController] No configuration selected, using fallback");
            CreateFallbackGridContent();
        }
    }
    
    /// <summary>
    /// Проверить, готова ли конфигурация для создания юнитов
    /// </summary>
    public bool IsConfigurationReady()
    {
        if (_configurationProvider == null)
        {
            return false;
        }
        
        var selectedConfig = _configurationProvider.GetSelectedConfiguration();
        return selectedConfig != null;
    }
    
    private void CreateFallbackGridContent()
    {
        if (gridContentEntrySO != null)
        {
            Debug.Log("[GameController] Using fallback configuration");
            CreateGridContent(gridContentEntrySO);
        }
        else
        {
            Debug.LogError("[GameController] No fallback configuration available!");
        }
    }

    public void CreateGridContent(GridContentEntrySO unitContentEntrySO)
    {
        if (unitContentEntrySO == null)
        {
            Debug.LogError("[GameController] GridContentEntrySO is null!");
            return;
        }
        
        // В сетевом режиме контент создаёт только хост
        if (NetworkManager.Singleton != null && !IsHost)
        {
            throw new InvalidOperationException("CreateGridContent can be called only by Host in network mode. Clients must wait for SpawnAcknowledgeClientRpc.");
        }
        if(_gateway == null)
            throw new InvalidOperationException("GameNetworkCommandGateway is null"); 
        Debug.Log($"[GameController] Creating grid content from config: {unitContentEntrySO.name}");
        
        foreach (var content in unitContentEntrySO.contents)
        {
            UnitSpawnParams unitSpawnParams = new UnitSpawnParams(content.X, content.Y, content.unitType, content.Amount, content.isPlayer);
            if (NetworkManager.Singleton != null)
            {
                if (!_gateway.TrySendSpawnUnit(unitSpawnParams.X, unitSpawnParams.Y, unitSpawnParams.UnitType, unitSpawnParams.Amount, unitSpawnParams.IsPlayer))
                {
                    Debug.LogWarning("[GameController] Gateway not spawned yet, deferring spawn to next frame");
                    // Можно поставить флаг и повторить попытку позже, пока просто логируем
                }
            }
            else
            {
                SpawnLocally(unitSpawnParams);
            }
        }
    }

    private void SpawnLocally(UnitSpawnParams unitSpawnParams)
    {
        _gameModel.SpawnUnit(unitSpawnParams);
    }

    public void Setup(int width, int height)
    {
        _gameModel.InitializeGrid(width, height);
    }

    private System.Collections.IEnumerator WaitForGatewayAndStart()
    {
        int safetyFrames = 120; // ~2 секунды при 60 FPS
        while ((_gateway == null || !_gateway.IsSpawned) && safetyFrames-- > 0)
        {
            yield return null;
        }
        if (_gateway == null || !_gateway.IsSpawned)
        {
            throw new InvalidOperationException("GameNetworkCommandGateway is not spawned in time");
        }
        CreateGridContentFromConfiguration();
        RunBattle();
    }
}
