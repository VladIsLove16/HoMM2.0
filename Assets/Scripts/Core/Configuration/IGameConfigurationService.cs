using System.Collections.Generic;
using UnityEngine;

public interface IGameConfigurationService
{
    bool HasConfiguration { get; }
    IReadOnlyList<GridContentEntrySO> GetAvailableConfigurations();
    GridContentEntrySO GetSelectedConfiguration();
    int GetSelectedConfigurationIndex();
    Team Team { get; }
    Team BattlefieldBottomTeam { get; }
    GameMode CurrentGameMode { get; }
    void SetSelectedConfiguration(int index);
    void SetTeam(Team team);
    void SetBattlefieldBottomTeam(Team team);
    void SetGameMode(GameMode mode);
    void Clear();
}
