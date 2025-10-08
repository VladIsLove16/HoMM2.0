using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface IGameConfigurationProvider
{
    GridContentEntrySO GetSelectedConfiguration();
    GridContentEntrySO GetConfigurationByIndex(int index);
    int GetSelectedConfigurationIndex();
    Team GetTeam();
    GameMode GetGameMode();
    Vector2Int GetGridSize();
}
public class GameConfigurationService : IGameConfigurationService
{
    public static GameConfigurationService Instance { get; } = new GameConfigurationService();

    private readonly List<GridContentEntrySO> _availableConfigs = new();
    private int _selectedConfigurationIndex = -1;
    private Team _team = Team.Blue;
    private GameMode _gameMode = GameMode.SinglePlayer;
    private Vector2Int _gridSize = new Vector2Int(10, 10);

    private GameConfigurationService()
    {
    }

    public event Action<GameConfigurationSnapshot> ConfigurationChanged;

    public bool HasConfiguration => _availableConfigs.Count > 0;

    public GameConfigurationSnapshot Snapshot => new GameConfigurationSnapshot(
        _availableConfigs.ToArray(),
        GetSelectedConfigurationIndex(),
        _team,
        _gameMode,
        _gridSize);

    public IReadOnlyList<GridContentEntrySO> GetAvailableConfigurations() => _availableConfigs.AsReadOnly();

    public GridContentEntrySO GetSelectedConfiguration()
    {
        var index = GetSelectedConfigurationIndex();
        if (index < 0 || index >= _availableConfigs.Count)
        {
            return null;
        }
        return _availableConfigs[index];
    }

    public int GetSelectedConfigurationIndex()
    {
        if (_availableConfigs.Count == 0)
        {
            return -1;
        }
        return Mathf.Clamp(_selectedConfigurationIndex, 0, _availableConfigs.Count - 1);
    }

    public Team Team => _team;

    public Vector2Int GridSize => _gridSize;

    public GameMode CurrentGameMode => _gameMode;

    public void SetAvailableConfigurations(IEnumerable<GridContentEntrySO> configs)
    {
        _availableConfigs.Clear();
        if (configs != null)
        {
            foreach (var config in configs)
            {
                if (config != null && !_availableConfigs.Contains(config))
                {
                    _availableConfigs.Add(config);
                }
            }
        }

        if (_availableConfigs.Count == 0)
        {
            _selectedConfigurationIndex = -1;
            _gridSize = new Vector2Int(10, 10);
        }
        else
        {
            _selectedConfigurationIndex = Mathf.Clamp(_selectedConfigurationIndex, 0, _availableConfigs.Count - 1);
            UpdateGridSizeFromSelection();
        }

        RaiseChanged();
    }

    public void SetSelectedConfiguration(int index)
    {
        if (_availableConfigs.Count == 0)
        {
            _selectedConfigurationIndex = -1;
            RaiseChanged();
            return;
        }

        var clamped = Mathf.Clamp(index, 0, _availableConfigs.Count - 1);
        if (clamped == _selectedConfigurationIndex)
        {
            return;
        }

        _selectedConfigurationIndex = clamped;
        UpdateGridSizeFromSelection();
        RaiseChanged();
    }

    public void SetTeam(Team team)
    {
        if (_team == team)
        {
            return;
        }

        _team = team;
        RaiseChanged();
    }

    public void SetGameMode(GameMode mode)
    {
        if (_gameMode == mode)
        {
            return;
        }

        _gameMode = mode;
        RaiseChanged();
    }

    public void SetGridSize(Vector2Int gridSize)
    {
        if (_gridSize == gridSize)
        {
            return;
        }

        _gridSize = new Vector2Int(Mathf.Max(1, gridSize.x), Mathf.Max(1, gridSize.y));
        RaiseChanged();
    }

    public void SetGridSize(int width, int height) => SetGridSize(new Vector2Int(width, height));

    public void Clear()
    {
        _availableConfigs.Clear();
        _selectedConfigurationIndex = -1;
        _team = Team.Blue;
        _gameMode = GameMode.SinglePlayer;
        _gridSize = new Vector2Int(10, 10);
        RaiseChanged();
    }

    private void UpdateGridSizeFromSelection()
    {
        var config = GetSelectedConfiguration();
        if (config != null)
        {
            _gridSize = new Vector2Int(
                Mathf.Max(1, config.Width),
                Mathf.Max(1, config.Height));
        }
    }

    private void RaiseChanged()
    {
        ConfigurationChanged?.Invoke(Snapshot);
    }
}public readonly struct GameConfigurationSnapshot
{
    public GameConfigurationSnapshot(
        IReadOnlyList<GridContentEntrySO> availableConfigurations,
        int selectedConfigurationIndex,
        Team team,
        GameMode gameMode,
        Vector2Int gridSize)
    {
        AvailableConfigurations = availableConfigurations ?? Array.Empty<GridContentEntrySO>();
        SelectedConfigurationIndex = selectedConfigurationIndex;
        Team = team;
        GameMode = gameMode;
        GridSize = gridSize;    
    }

    public IReadOnlyList<GridContentEntrySO> AvailableConfigurations { get; }
    public int SelectedConfigurationIndex { get; }
    public Team Team { get; }
    public GameMode GameMode { get; }
    public Vector2Int GridSize { get; }

    public bool HasConfiguration => AvailableConfigurations.Count > 0;

    public GridContentEntrySO SelectedConfiguration =>
        SelectedConfigurationIndex >= 0 && SelectedConfigurationIndex < AvailableConfigurations.Count
            ? AvailableConfigurations[SelectedConfigurationIndex]
            : null;
}

public interface IGameConfigurationService : IGameModeProvider
{
    event Action<GameConfigurationSnapshot> ConfigurationChanged;

    bool HasConfiguration { get; }
    GameConfigurationSnapshot Snapshot { get; }
    IReadOnlyList<GridContentEntrySO> GetAvailableConfigurations();
    GridContentEntrySO GetSelectedConfiguration();
    int GetSelectedConfigurationIndex();
    Team Team { get; }
    Vector2Int GridSize { get; }

    void SetAvailableConfigurations(IEnumerable<GridContentEntrySO> configs);
    void SetSelectedConfiguration(int index);
    void SetTeam(Team team);
    void SetGameMode(GameMode mode);
    void SetGridSize(Vector2Int gridSize);
    void SetGridSize(int width, int height);
    void Clear();
}

/// <summary>
/// Scene-level configuration provider that either consumes a snapshot supplied by the lobby
/// via <see cref="GameConfigurationService"/> or falls back to inspector defaults when none exist.
/// </summary>
public class SceneGameConfigurationProvider : MonoBehaviour, IGameConfigurationProvider
{
    [Header("Default configuration (used when no snapshot supplied)")]
    [SerializeField] private List<GridContentEntrySO> defaultConfigurations = new();
    [SerializeField] private int defaultSelectedIndex;
    [SerializeField] private GameMode defaultGameMode = GameMode.SinglePlayer;
    [SerializeField] private Team defaultTeam = Team.Blue;
    [SerializeField] private Vector2Int fallbackGridSize = new Vector2Int(10, 10);
    [SerializeField] private bool overrideExistingOnAwake;

    private IGameConfigurationService _configurationService;

    [Inject]
    public void Construct(IGameConfigurationService configurationService)
    {
        _configurationService = configurationService ?? GameConfigurationService.Instance;
    }

    private void Awake()
    {
        _configurationService ??= GameConfigurationService.Instance;

        if (!_configurationService.HasConfiguration || overrideExistingOnAwake)
        {
            ApplyDefaults();
        }
        else if (defaultConfigurations.Count == 0)
        {
            EnsureGridSize();
        }
    }

    /// <summary>
    /// Allows external callers (e.g., lobby flow) to push a snapshot into the shared service
    /// before gameplay systems query it.
    /// </summary>
    public void ApplySnapshot(GameConfigurationSnapshot snapshot)
    {
        if (snapshot.AvailableConfigurations != null)
        {
            _configurationService.SetAvailableConfigurations(snapshot.AvailableConfigurations);
        }
        _configurationService.SetSelectedConfiguration(snapshot.SelectedConfigurationIndex);
        _configurationService.SetTeam(snapshot.Team);
        _configurationService.SetGameMode(snapshot.GameMode);
        _configurationService.SetGridSize(snapshot.GridSize);
    }

    private void ApplyDefaults()
    {
        if (defaultConfigurations == null)
        {
            defaultConfigurations = new List<GridContentEntrySO>();
        }

        _configurationService.SetAvailableConfigurations(defaultConfigurations);
        if (defaultConfigurations.Count > 0)
        {
            var index = Mathf.Clamp(defaultSelectedIndex, 0, defaultConfigurations.Count - 1);
            _configurationService.SetSelectedConfiguration(index);
        }
        else
        {
            _configurationService.SetSelectedConfiguration(-1);
            EnsureGridSize();
        }

        _configurationService.SetTeam(defaultTeam);
        _configurationService.SetGameMode(defaultGameMode);
    }

    private void EnsureGridSize()
    {
        _configurationService.SetGridSize(fallbackGridSize);
    }

    public GridContentEntrySO GetSelectedConfiguration() => _configurationService.GetSelectedConfiguration();

    public GridContentEntrySO GetConfigurationByIndex(int index)
    {
        var configs = _configurationService.GetAvailableConfigurations();
        if (index >= 0 && index < configs.Count)
        {
            return configs[index];
        }
        return null;
    }

    public int GetSelectedConfigurationIndex() => _configurationService.GetSelectedConfigurationIndex();

    public Team GetTeam() => _configurationService.Team;

    public GameMode GetGameMode() => _configurationService.CurrentGameMode;

    public Vector2Int GetGridSize() => _configurationService.GridSize;
}
