// MaterialProvider.cs
using UnityEngine;
// Реальная реализация провайдера материалов
[System.Serializable]
public class MaterialProvider : IMaterialProvider, ITeamColorProvider
{
    [SerializeField] private Material _blueTeamMaterial;
    [SerializeField] private Material _hoveredBlueTeamMaterial;
    [SerializeField] private Material _redTeamMaterial;
    [SerializeField] private Material _hoveredRedTeamMaterial;
    [SerializeField] private Material _greenTeamMaterial;
    [SerializeField] private Material _hoveredGreenTeamMaterial;
    [SerializeField] private Material _yellowTeamMaterial;
    [SerializeField] private Material _hoveredYellowTeamMaterial;
    [SerializeField] private Color _blueTeamColor = Color.blue;
    [SerializeField] private Color _hoveredBlueTeamColor = new Color(0.4f, 0.7f, 1f);
    [SerializeField] private Color _redTeamColor = Color.red;
    [SerializeField] private Color _hoveredRedTeamColor = new Color(1f, 0.5f, 0.5f);
    [SerializeField] private Color _greenTeamColor = Color.green;
    [SerializeField] private Color _hoveredGreenTeamColor = new Color(0.4f, 1f, 0.4f);
    [SerializeField] private Color _yellowTeamColor = Color.yellow;
    [SerializeField] private Color _hoveredYellowTeamColor = new Color(1f, 1f, 0.5f);

    public Material GetTeamMaterial(Team team)
    {
        return team switch
        {
            Team.Blue => _blueTeamMaterial,
            Team.Red => _redTeamMaterial,
            Team.Green => _greenTeamMaterial != null ? _greenTeamMaterial : _blueTeamMaterial,
            Team.Yellow => _yellowTeamMaterial != null ? _yellowTeamMaterial : _blueTeamMaterial,
            _ => _blueTeamMaterial
        };
    }

    public Material GetHoveredTeamMaterial(Team team)
    {
        return team switch
        {
            Team.Blue => _hoveredBlueTeamMaterial,
            Team.Red => _hoveredRedTeamMaterial,
            Team.Green => _hoveredGreenTeamMaterial != null ? _hoveredGreenTeamMaterial : _hoveredBlueTeamMaterial,
            Team.Yellow => _hoveredYellowTeamMaterial != null ? _hoveredYellowTeamMaterial : _hoveredBlueTeamMaterial,
            _ => _hoveredBlueTeamMaterial
        };
    }
    public Material GetBlueTeamMaterial() => _blueTeamMaterial;
    public Material GetHoveredBlueTeamMaterial() => _hoveredBlueTeamMaterial;
    public Material GetRedTeamMaterial() => _redTeamMaterial;
    public Material GetHoveredRedTeamMaterial() => _hoveredRedTeamMaterial;
    public Material GetGreenTeamMaterial() => _greenTeamMaterial;
    public Material GetHoveredGreenTeamMaterial() => _hoveredGreenTeamMaterial;
    public Material GetYellowTeamMaterial() => _yellowTeamMaterial;
    public Material GetHoveredYellowTeamMaterial() => _hoveredYellowTeamMaterial;

    public Material BlueTeamMaterial => _blueTeamMaterial;
    public Material HoveredBlueTeamMaterial => _hoveredBlueTeamMaterial;
    public Material RedTeamMaterial => _redTeamMaterial;
    public Material HoveredRedTeamMaterial => _hoveredRedTeamMaterial;
    public Material GreenTeamMaterial => _greenTeamMaterial;
    public Material HoveredGreenTeamMaterial => _hoveredGreenTeamMaterial;
    public Material YellowTeamMaterial => _yellowTeamMaterial;
    public Material HoveredYellowTeamMaterial => _hoveredYellowTeamMaterial;

    public Color GetTeamColor(Team team)
    {
        return team switch
        {
            Team.Blue => _blueTeamColor,
            Team.Red => _redTeamColor,
            Team.Green => _greenTeamColor,
            Team.Yellow => _yellowTeamColor,
            _ => _blueTeamColor
        };
    }

    public Color GetHoveredTeamColor(Team team)
    {
        return team switch
        {
            Team.Blue => _hoveredBlueTeamColor,
            Team.Red => _hoveredRedTeamColor,
            Team.Green => _hoveredGreenTeamColor,
            Team.Yellow => _hoveredYellowTeamColor,
            _ => _hoveredBlueTeamColor
        };
    }
}

