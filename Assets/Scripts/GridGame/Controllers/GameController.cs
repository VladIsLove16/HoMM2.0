using NaughtyAttributes;
using System;
using UnityEngine;
using Zenject;

public class GameController : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private GameConfigurationService _configurationService;
    private GameModel _gameModel;
    private ITurnService _turnService;

    [Inject]
    public void Construct(
        GameModel model,
        ITurnService turnService)
    {
        _gameModel = model;
        _turnService = turnService;
        UnityLogger.Log("GameController inited with " + model.ToString());
    }

    private void Start()
    {
        Setup(_configurationService);
    }

    [Button]
    private void Setup()
    {
        Setup(_configurationService);
    }

    public void Setup(IGameConfigurationService configurationService)
    {
        if (_gameModel == null || _turnService == null)
        {
            Debug.LogError("[GameController] Missing dependencies. Ensure Zenject binding executed before Setup.", this);
            return;
        }

        var service = configurationService ?? _configurationService;
        var config = service?.GetSelectedConfiguration();
        CreateGridContent(config);
        ConfigureTurnControl(service);
        _turnService.StartGridPlacementPhase();
    }

    [Button]
    public void CreateGridContent()
    {
        var config = _configurationService.GetSelectedConfiguration();
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

    private void ConfigureTurnControl(IGameConfigurationService service)
    {
        var team = service?.Team ?? Team.Blue;
        var mode = service?.CurrentGameMode ?? GameMode.SinglePlayer;
        _turnService.ConfigureControl(team, mode);
    }
}
