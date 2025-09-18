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
    [SerializeField] GameConfigurationProviderSO gameConfigurationProvider;

    private GameModel _gameModel;
    private TurnSystem _turnSystem;
    private IBattleEntryProvider _battleEntryProvider;
    private IGameStartupFlow _startupFlow;
    private IUnitSpawner _unitSpawner;
    private IBattleRunner _battleRunner;
    private bool _isHost;
    [SerializeField] private Material _hostBlueTeamMaterial;

    [Inject]
    public void Construct(
        GameModel model,
        TurnSystem combatSystem,
        IBattleEntryProvider configurationProvider,
        IGameStartupFlow startupFlow,
        IUnitSpawner unitSpawner,
        IBattleRunner battleRunner)
    {
        _gameModel = model;
        _turnSystem = combatSystem;
        _battleEntryProvider = configurationProvider;
        _startupFlow = startupFlow;
        _unitSpawner = unitSpawner;
        _battleRunner = battleRunner;
    }

    public void Initialize()
    {
        Debug.Log("[GameController] Initialized with configuration provider");
    }

    public void SetIsHostFlag(bool isHost)
    {
        _isHost = isHost;
    }

    private void Start()
    {
        Debug.Log("[GameController] Start called. Flow: " + _startupFlow.ToString());
        _startupFlow.Run(); // теперь запускается напрямую
    }

    public void Setup()
    {
        Setup(gameConfigurationProvider.Width, gameConfigurationProvider.Height);
    }
        
    public void Setup(int width, int height)
    {
        _gameModel.InitializeGrid(width, height);
    }
    public void CreateGridContent()
    {
        CreateGridContent(gameConfigurationProvider.gridContentEntrySO);
    }
    public void CreateGridContent(GridContentEntrySO unitContentEntrySO)
    {
        foreach (var content in unitContentEntrySO.contents)
        {
            var spawnParams = new UnitSpawnParams(content.X, content.Y, content.unitType, content.Amount, content.isPlayer);
            _gameModel.SpawnUnit(spawnParams);
        }
    }

    public void CreateGridContentFromConfiguration()
    {
        var selectedConfig = _battleEntryProvider?.GetSelectedConfiguration();
        if (selectedConfig != null)
        {
            Debug.Log($"[GameController] Creating grid content from config: {selectedConfig.name}");
            CreateGridContent(selectedConfig);
        }
        else if (gameConfigurationProvider.gridContentEntrySO != null)
        {
            Debug.Log("[GameController] Using fallback config");
            CreateGridContent(gameConfigurationProvider.gridContentEntrySO);
        }
    }

    public void InitTurnSystem()
    {
        Debug.Log("[GameController] InitTurnSystem with units count in model: " + _gameModel.GetUnits().Count);
        _turnSystem.ClearUnits();
        foreach (var unit in _gameModel.GetUnits())
            _turnSystem.AddCombatUnit(unit);

        _gameModel.UnitSpawned += p => _turnSystem.AddCombatUnit(p.UnitModel);
        _gameModel.UnitDied += p => _turnSystem.RemoveCombatUnit(p.UnitModel);

        // Синий цвет закреплён за хостом
        _turnSystem.ConfigureLocalSide(isHostBlueTeam: _isHost, myTeamMaterial: _hostBlueTeamMaterial);
    }

    public void RunBattle()
    {
        _battleRunner.RunBattle();
    }

    public bool IsConfigurationReady()
    {
        throw new NotImplementedException();
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
    private readonly IBattleEntryProvider _config;

    public NetworkBattleRunner(TurnSystem combatSystem, IBattleEntryProvider config)
    {
        _combatSystem = combatSystem;
        _config = config;
    }

    public void RunBattle()
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= 2
            || _config.AcceptStartingBattleWithoutClients)
        {
            Debug.Log("_combatSystem.RunBattle");
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
