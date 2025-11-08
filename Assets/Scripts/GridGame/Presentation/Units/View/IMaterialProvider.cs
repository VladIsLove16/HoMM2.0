// IMaterialProvider.cs
using UnityEngine;

public interface IMaterialProvider
{
    Material GetTeamMaterial(Team team);
    Material GetHoveredTeamMaterial(Team team);
    Material GetBlueTeamMaterial();
    Material GetHoveredBlueTeamMaterial();
    Material GetRedTeamMaterial();
    Material GetHoveredRedTeamMaterial();
}

