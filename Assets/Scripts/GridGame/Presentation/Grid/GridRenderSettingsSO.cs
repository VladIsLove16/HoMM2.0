using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Grid/Render Settings", fileName = "GridRenderSettings")]
public class GridRenderSettingsSO : ScriptableObject, IGridRenderSettings
{
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float cellHeight = 1f;
    [SerializeField] private float cellPadding = 0.4f;
    [SerializeField] private List<CellMaterial> materials = new();

    public float CellSize => cellSize;
    public float CellHeight => cellHeight;
    public float CellPadding => cellPadding;
    public IReadOnlyList<CellMaterial> Materials => materials;
}
