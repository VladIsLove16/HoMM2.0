using UnityEngine;

public interface ITeamColorProvider
{
    Color GetTeamColor(Team team);
    Color GetHoveredTeamColor(Team team);
}
