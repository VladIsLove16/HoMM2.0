using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Immutable view of the currently selected game configuration.
/// </summary>
public class GameConfigurationSnapshot
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
    public IReadOnlyList<GridContentEntrySO> AvailableConfigurations;
    public int SelectedConfigurationIndex;
    public Team Team;
    public GameMode GameMode;
    public Vector2Int GridSize;

    public bool HasConfiguration => AvailableConfigurations.Count > 0;

    public GridContentEntrySO SelectedConfiguration =>
        SelectedConfigurationIndex >= 0 && SelectedConfigurationIndex < AvailableConfigurations.Count
            ? AvailableConfigurations[SelectedConfigurationIndex]
            : null;
}