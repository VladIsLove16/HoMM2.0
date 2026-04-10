using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(fileName = "GameConfigurationService", menuName = "Game/GameConfigurationService")]
public class GameConfigurationService : ScriptableObject, IGameConfigurationService
{
    [SerializeField] private List<GridContentEntrySO> _configs = new();
    [SerializeField] private ArmyLineupSO PlayerStartArmy;
    [SerializeField] private int _selectedIndex = 0;
    [SerializeField] private Team _team = Team.Blue;
    [SerializeField] private Team _battlefieldBottomTeam = Team.Blue;
    [SerializeField] private GameMode _mode = GameMode.SinglePlayer;
    public List<GridContentEntrySO> AvailableConfigs => _configs;
    public bool HasConfiguration => _configs.Count > 0;
    public Team Team => _team;
    public Team BattlefieldBottomTeam => _battlefieldBottomTeam;
    public GameMode CurrentGameMode => _mode;

    public IReadOnlyList<GridContentEntrySO> GetAvailableConfigurations() => _configs;
    public GridContentEntrySO GetSelectedConfiguration() =>
        _selectedIndex >= 0 && _selectedIndex < _configs.Count ? _configs[_selectedIndex] : null;
    public int GetSelectedConfigurationIndex() => _selectedIndex;
    public ArmyLineupSO GetPlayerArmy() => PlayerStartArmy;
    public void SetAvailableConfigurations(List<GridContentEntrySO> configs)
    {
        _configs = configs?.ToList() ?? new List<GridContentEntrySO>();
    }
    public void SetSelectedConfiguration(int index) => _selectedIndex = index;
    public void SetTeam(Team team) => _team = team;
    public void SetBattlefieldBottomTeam(Team team) => _battlefieldBottomTeam = team;
    public void SetGameMode(GameMode mode) => _mode = mode;
    public void Clear() => _configs.Clear();
}
