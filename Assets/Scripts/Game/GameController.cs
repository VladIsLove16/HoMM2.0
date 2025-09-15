using NaughtyAttributes;
using System;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using static UnityEngine.EventSystems.EventTrigger;

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
    [SerializeField] private GameNetworkCommandGateway _gateway;

    private GameModel _gameModel;
    private TurnSystem _turnSystem;
    private IGameConfigurationProvider _configurationProvider;
    private IGameStartupFlow _startupFlow;
    private IUnitSpawner _unitSpawner;
    private IBattleRunner _battleRunner;

    [Inject]
    public void Construct(
        GameViewModel viewModel, 
        GridView view, 
        GameModel model, 
        TurnSystem combatSystem,
        IGameConfigurationProvider configurationProvider,
        IGameStartupFlow startupFlow,
         IUnitSpawner unitSpawner,
        IBattleRunner battleRunner)
    {
        _gameModel = model;
        _turnSystem = combatSystem;
        _configurationProvider = configurationProvider;
        _startupFlow = startupFlow;
        _unitSpawner = unitSpawner;
        _battleRunner = battleRunner; 
    }
    
    public void Initialize()
    {
        Debug.Log("[GameController] Initialized with configuration provider");
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("[GameController] OnNetworkSpawn called");
        Debug.Log($"[GameController] NetworkObjectId: {NetworkObjectId}, IsHost: {IsHost}, IsClient: {IsClient}");
        _startupFlow.Run();
    }
    
    private void Start()
    {
        Debug.Log("[GameController] Start called");
    }

    public void InitCombatSystem()
    {
        // Синхронизируем текущих юнитов
        _turnSystem.ClearUnits();
        foreach (var unit in _gameModel.GetUnits())
        {
            _turnSystem.AddCombatUnit(unit);
        }

        _gameModel.UnitSpawned += OnGameModel_UnitSpawned;
        _gameModel.UnitDied += OnGameModel_UnitDied;
    }

    private void OnGameModel_UnitDied(ContentDiedParams @params)
    {
        if (@params?.UnitModel != null)
        {
            _turnSystem.RemoveCombatUnit(@params.UnitModel);
           var state =  _turnSystem.BattleState;
            if(state == BattleState.blueTeamWins)
            {
                
            }
            else if(state == BattleState.redTeamWins)
            {

            }
        }
    }

    private void OnGameModel_UnitSpawned(UnitModelCreatedParams @params)
    {
        if (@params?.UnitModel != null)
        {
            _turnSystem.AddCombatUnit(@params.UnitModel);
        }
    }

    [Button]
    public void Setup()
    {
        Setup(Width, Height);
    }
    public void Setup(int width, int height)
    {
        _gameModel.InitializeGrid(width, height);
    }
    [Button]
    public void RunBattle()
    {
        _battleRunner.RunBattle();
    }

    [Button]
    public void CreateGridContent()
    {
        CreateGridContent(gridContentEntrySO);
    }

    public void CreateGridContentFromConfiguration()
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
        foreach (var content in unitContentEntrySO.contents)
        {
            var spawnParams = new UnitSpawnParams(content.X, content.Y, content.unitType, content.Amount, content.isPlayer);
            _unitSpawner.SpawnUnit(spawnParams);
        }
    }


}
public class LocalUnitSpawner : IUnitSpawner
{
    private readonly GameModel _gameModel;

    public LocalUnitSpawner(GameModel gameModel)
    {
        _gameModel = gameModel;
    }

    public void SpawnUnit(UnitSpawnParams spawnParams)
    {
        _gameModel.SpawnUnit(spawnParams);
    }
}

public class LocalBattleRunner : IBattleRunner
{
    private readonly TurnSystem _combatSystem;

    public LocalBattleRunner(TurnSystem combatSystem)
    {
        _combatSystem = combatSystem;
    }

    public void RunBattle()
    {
        _combatSystem.RunBattle();
    }
}
public class NetworkUnitSpawner : IUnitSpawner
{
    private readonly GameNetworkCommandGateway _gateway;

    public NetworkUnitSpawner(GameNetworkCommandGateway gateway)
    {
        _gateway = gateway;
    }

    public void SpawnUnit(UnitSpawnParams spawnParams)
    {
        if (!_gateway.TrySendSpawnUnit(spawnParams.X, spawnParams.Y, spawnParams.UnitType, spawnParams.Amount, spawnParams.IsPlayer))
        {
            Debug.LogWarning("[NetworkUnitSpawner] Gateway not ready, deferring spawn");
        }
    }
}

public class NetworkBattleRunner : IBattleRunner
{
    private readonly TurnSystem _combatSystem;
    private readonly IGameConfigurationProvider _config;

    public NetworkBattleRunner(TurnSystem combatSystem, IGameConfigurationProvider config)
    {
        _combatSystem = combatSystem;
        _config = config;
    }

    public void RunBattle()
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= 2
            || _config.AcceptStartingBattleWithoutClients)
        {
            _combatSystem.RunBattle();
        }
        else
        {
            Debug.Log("Wait for clients...");
        }
    }
}
public interface IUnitSpawner
{
    void SpawnUnit(UnitSpawnParams spawnParams);
}

public interface IBattleRunner
{
    void RunBattle();
}
