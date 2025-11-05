// MaterialProvider.cs
using UnityEngine;
// Реальная реализация провайдера материалов
[System.Serializable]
public class MaterialProvider : IMaterialProvider
{
    [SerializeField] private Material _blueTeamMaterial;
    [SerializeField] private Material _hoveredBlueTeamMaterial;
    [SerializeField] private Material _redTeamMaterial;
    [SerializeField] private Material _hoveredRedTeamMaterial;

    public Material GetTeamMaterial(Team team) => team == Team.Blue ? _blueTeamMaterial : _redTeamMaterial;

    public Material GetHoveredTeamMaterial(Team team) => team == Team.Blue ? _hoveredBlueTeamMaterial : _hoveredRedTeamMaterial;
    public Material GetBlueTeamMaterial() => _blueTeamMaterial;
    public Material GetHoveredBlueTeamMaterial() => _hoveredBlueTeamMaterial;
    public Material GetRedTeamMaterial() => _redTeamMaterial;
    public Material GetHoveredRedTeamMaterial() => _hoveredRedTeamMaterial;

    public Material BlueTeamMaterial => _blueTeamMaterial;
    public Material HoveredBlueTeamMaterial => _hoveredBlueTeamMaterial;
    public Material RedTeamMaterial => _redTeamMaterial;
    public Material HoveredRedTeamMaterial => _hoveredRedTeamMaterial;
}

