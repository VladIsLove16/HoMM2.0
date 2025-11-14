using UnityEngine;

[System.Serializable]
public class CellMaterial
{
    public CellMaterial()
    {
    }

    public CellMaterial(CellState cellState, Material material)
    {
        CellState = cellState;
        Material = material;
    }

    [SerializeField] private CellState cellState;
    [SerializeField] private Material material;

    public CellState CellState
    {
        get => cellState;
        set => cellState = value;
    }

    public Material Material
    {
        get => material;
        set => material = value;
    }
}
