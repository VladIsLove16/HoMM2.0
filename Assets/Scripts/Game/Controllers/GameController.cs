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
    [Inject] private IGameConfigurationProvider _gameConfigurationProvider;
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
        Setup(_gameConfigurationProvider);
    }

    [Button]
    private void Setup()
    {
        Setup(_gameConfigurationProvider);
    }

    public void Setup(IGameConfigurationProvider gameConfigurationProvider)
    {
        if (gameConfigurationProvider == null)
            throw new ArgumentNullException();
        var provider = gameConfigurationProvider;
        var config = provider?.GetSelectedConfiguration();
        CreateGridContent(config);
        var team = provider.GetTeam();
        SetPlayerTeam(team);
        _turnService.StartGridPlacementPhase();
    }

    [Button]
    public void CreateGridContent()
    {
        var config = _gameConfigurationProvider.GetSelectedConfiguration();
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

    private void SetPlayerTeam(Team team)
    {
        _turnService.ConfigureLocalSide(team);
    }
}



