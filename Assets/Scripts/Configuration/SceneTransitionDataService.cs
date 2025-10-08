using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Legacy MonoBehaviour kept for scene compatibility.
/// Acts as a bootstrapper that feeds default configuration data into <see cref="GameConfigurationService"/>.
/// </summary>
public class SceneTransitionDataService : MonoBehaviour, IGameModeProvider
{
    [Header("Bootstrapped configuration")]
    [SerializeField] private GridContentEntrySO[] _defaultConfigurations = new GridContentEntrySO[0];
    [SerializeField] private int _defaultSelectedConfigIndex = 0;
    [SerializeField] private GameMode _defaultGameMode = GameMode.SinglePlayer;
    [SerializeField] private Team _defaultTeam = Team.Blue;
    [SerializeField] private Vector2Int _defaultGridSize = new Vector2Int(10, 10);
    [SerializeField] private bool _applyOnAwake = true;
    [SerializeField] private bool _overrideExistingConfiguration;
    [SerializeField] private bool _dontDestroyOnLoad = true;
    [SerializeField] private bool acceptStartingGameWithoutClients;

    public bool AcceptStartingGameWithoutClients => acceptStartingGameWithoutClients;

    private GameConfigurationService Service => GameConfigurationService.Instance;

    private void Awake()
    {
        if (_dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        if (_applyOnAwake)
        {
            ApplyDefaults(_overrideExistingConfiguration);
        }
    }

    /// <summary>
    /// Applies the serialized defaults to the shared configuration service.
    /// </summary>
    public void ApplyDefaults(bool force)
    {
        if (force || !Service.HasConfiguration)
        {
            Service.SetAvailableConfigurations(_defaultConfigurations);
            Service.SetSelectedConfiguration(_defaultSelectedConfigIndex);
        }

        if (force || !Service.HasConfiguration)
        {
            Service.SetTeam(_defaultTeam);
            Service.SetGameMode(_defaultGameMode);
            Service.SetGridSize(_defaultGridSize);
        }
    }

    public void SetAvailableConfigurations(IEnumerable<GridContentEntrySO> configs, int selectedIndex = 0)
    {
        Service.SetAvailableConfigurations(configs);
        Service.SetSelectedConfiguration(selectedIndex);
    }

    public void SetSelectedConfiguration(int configIndex)
    {
        Service.SetSelectedConfiguration(configIndex);
    }

    public void SetGameMode(GameMode gameMode)
    {
        Service.SetGameMode(gameMode);
    }

    public void SetTeam(Team team)
    {
        Service.SetTeam(team);
    }

    public GridContentEntrySO GetSelectedConfiguration()
    {
        return Service.GetSelectedConfiguration();
    }

    public GridContentEntrySO GetConfigurationByIndex(int index)
    {
        var configs = Service.GetAvailableConfigurations();
        if (index >= 0 && index < configs.Count)
        {
            return configs[index];
        }
        return null;
    }

    public int GetSelectedConfigurationIndex()
    {
        return Service.GetSelectedConfigurationIndex();
    }

    public GridContentEntrySO[] GetAvailableConfigurations()
    {
        return Service.GetAvailableConfigurations().ToArray();
    }

    public void ReloadConfigurations()
    {
        ApplyDefaults(force: false);
    }

    public GameMode CurrentGameMode => Service.CurrentGameMode;
}
