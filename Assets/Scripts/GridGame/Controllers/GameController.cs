using NaughtyAttributes;
using System;
using UnityEngine;
using Zenject;

public class GameController : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private SinglePlayerStartConfigurationSO _startConfiguration;
    private GameModel _gameModel;
    private ITurnService _turnService;
    private SinglePlayerStartConfigurationSO _injectedStartConfiguration;

    [Inject]
    public void Construct(
        GameModel model,
        ITurnService turnService,
        [InjectOptional] SinglePlayerStartConfigurationSO startConfiguration = null)
    {
        _gameModel = model;
        _turnService = turnService;
        _injectedStartConfiguration = startConfiguration;
        UnityLogger.Log("GameController inited with " + model.ToString());
    }

    private void Start()
    {
        Setup(ResolveStartConfiguration());
    }

    [Button]
    private void Setup()
    {
        Setup(ResolveStartConfiguration());
    }

    public void Setup(SinglePlayerStartConfigurationSO startConfiguration)
    {
        if (_gameModel == null || _turnService == null)
        {
            Debug.LogError("[GameController] Missing dependencies. Ensure Zenject binding executed before Setup.", this);
            return;
        }

        var activeConfiguration = startConfiguration ?? ResolveStartConfiguration();
        if (activeConfiguration == null)
        {
            Debug.LogError("[GameController] SinglePlayerStartConfiguration is not assigned. Cannot setup battle grid.", this);
            return;
        }

        activeConfiguration.EnsureSelectedBattleConfiguration();
        var config = activeConfiguration.GetSelectedConfiguration();
        CreateGridContent(config);
        ConfigureTurnControl(activeConfiguration);
        _turnService.StartGridPlacementPhase();
    }

    [Button]
    public void CreateGridContent()
    {
        var activeConfiguration = ResolveStartConfiguration();
        activeConfiguration?.EnsureSelectedBattleConfiguration();
        var config = activeConfiguration != null ? activeConfiguration.GetSelectedConfiguration() : null;
        if (config == null)
        {
            Debug.LogWarning("CreateGridContent called with null configuration. Skipping grid setup.");
            return;
        }
        CreateGridContent(config);
    }

    public void CreateGridContent(GridContentEntrySO unitContentEntrySO)
    {
        if (_gameModel == null)
        {
            Debug.LogError("[GameController] GameModel is not initialized. Cannot create grid content.", this);
            return;
        }

        if (unitContentEntrySO == null)
        {
            Debug.LogWarning("CreateGridContent called with null configuration. Skipping grid setup.");
            return;
        }

        _gameModel.InitializeGrid(unitContentEntrySO.Width, unitContentEntrySO.Height);
        _gameModel.GameChange_UnitSpawned += OnGameModel_UnitSpawn;
        var contents = unitContentEntrySO.contents;
        if (contents == null || contents.Count == 0)
        {
            Debug.LogWarning("[GameController] Grid content entry has no units configured.", this);
            return;
        }

        foreach (var content in contents)
        {
            var team = content.Team;
            var spawnParams = new UnitSpawnParams(content.X, content.Y, content.unitType, content.Amount, team);
            _gameModel.SpawnUnit(spawnParams);
        }
    }

    private void OnGameModel_UnitSpawn(UnitModelCreatedParams @params)
    {
        _turnService.AddCombatUnit(@params.UnitModel);
    }

    private void ConfigureTurnControl(SinglePlayerStartConfigurationSO startConfiguration)
    {
        var team = startConfiguration?.PlayerTeam ?? Team.Blue;
        var mode = startConfiguration?.CurrentGameMode ?? GameMode.SinglePlayer;
        _turnService.ConfigureControl(team, mode);
    }

    private SinglePlayerStartConfigurationSO ResolveStartConfiguration()
    {
        return _injectedStartConfiguration != null ? _injectedStartConfiguration : _startConfiguration;
    }
}
