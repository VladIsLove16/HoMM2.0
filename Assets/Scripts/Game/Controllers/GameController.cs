using NaughtyAttributes;
using System;
using UnityEngine;
using Zenject;

public class GameController : MonoBehaviour
{
    [Header("Grid Settings")]
    [Inject] private IGameConfigurationService _configurationService;
    private GameModel _gameModel;
    private ITurnService _turnService;

    [Inject]
    public void Construct(
        GameModel model,
        ITurnService turnService)
    {
        _gameModel = model;
        _turnService = turnService;
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
        CreateGridContent(config);
    }

    public void CreateGridContent(GridContentEntrySO unitContentEntrySO)
    {
        if (unitContentEntrySO == null)
        {
            Debug.LogWarning("CreateGridContent called with null configuration. Skipping grid setup.");
            return;
        }

        _gameModel.InitializeGrid(unitContentEntrySO.Width, unitContentEntrySO.Height);
        _gameModel.GameChange_UnitSpawned += OnGameModel_UnitSpawn;
        foreach (var content in unitContentEntrySO.contents)
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
