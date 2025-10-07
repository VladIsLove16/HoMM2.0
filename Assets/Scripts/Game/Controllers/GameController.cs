using NaughtyAttributes;
using System;
using UnityEngine;
using Zenject;
/// <summary>
/// Основной контроллер игры
/// Управляет инициализацией игрового процесса и созданием юнитов
/// </summary>
public class GameController : MonoBehaviour
{
    [Header("Grid Settings")]
    [Inject] private GameSceneConfigurationProvider _gameSceneConfigurationProvider;
    [SerializeField] private GridContentEntrySO defaultEntry;
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
        Setup(_gameSceneConfigurationProvider);
    }

    [Button]
    private void Setup()
    {
        Setup(_gameSceneConfigurationProvider);
    }

    public void Setup(GameSceneConfigurationProvider gameConfigurationProvider)
    {
        GridContentEntrySO config;
        config = gameConfigurationProvider == null ? defaultEntry : gameConfigurationProvider.GetSelectedConfiguration();
        CreateGridContent(config);
        var team = gameConfigurationProvider == null ? Team.Blue : gameConfigurationProvider.GetTeam();
        SetPlayerTeam(team);
        _turnService.StartGridPlacementPhase();
    }

    [Button]
    public void CreateGridContent()
    {
        CreateGridContent(_gameSceneConfigurationProvider.GetSelectedConfiguration());
    }

    public void CreateGridContent(GridContentEntrySO unitContentEntrySO)
    {
        _gameModel.InitializeGrid(unitContentEntrySO.Width, unitContentEntrySO.Height);
        _gameModel.GameChange_UnitSpawned += OnGameModel_UnitSpawn;
        foreach (var content in unitContentEntrySO.contents)
        {
            // content.Team is a serialized bool in grid entries; translate to Team
            var team = content.Team;
            var spawnParams = new UnitSpawnParams(content.X, content.Y, content.unitType, content.Amount, team);
            _gameModel.SpawnUnit(spawnParams);
        }
    }

    private void OnGameModel_UnitSpawn(UnitModelCreatedParams @params)
    {
        _turnService.AddCombatUnit(@params.UnitModel);
    }

    private void SetPlayerTeam(Team team)
    {
        _turnService.ConfigureLocalSide(team);
    }
}
