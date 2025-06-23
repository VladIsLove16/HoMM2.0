using System.Collections.Generic;
using System;
using UnityEngine;
using static UnityEngine.UI.Image;
using System.Linq;

public class PerCellGridRenderer : ICellGridRenderer
{
    [Serializable]
    public class CellMaterials
    {
        public CellState CellState;
        public Material Material;

    }
    List<GameObject> instances = new List<GameObject>();
    GameObject prefab;
    GameObject parent;
    float ySize = 1f;
    Grid<GameObject> grid;
    private Dictionary<CellState,CellMaterials> materials = new ();
    private Dictionary<Vector2Int,CellState> cellStates = new ();
    public PerCellGridRenderer(GameObject prefab, GameObject parent, List<CellMaterials> materials)
    {
        this.parent = parent;
        this.prefab = prefab;
        this.materials = materials.ToDictionary(x => x.CellState);
    }

    public void Clear()
    {
        foreach (GameObject child in instances)
        {
            GameObject.Destroy(child);
        }
    }

    public void Render(int width, int height, float cellSize,Vector3 origin, float padding)
    {
        Clear();
        grid = new Grid<GameObject>(width, height, cellSize, origin, padding, CreateCellView);
    }

    public void SetCellState(Vector2Int cellCoords, CellState v)
    {
        GameObject cell = grid.GetGridObject(cellCoords.x, cellCoords.y);
        cell.GetComponent<Renderer>().material = materials[v].Material;
        cellStates[cellCoords] = v;
    }

    public bool ToGrid(Vector3 position,out Vector2Int coords)
    {
        coords = grid.GetXY(position);
        if (grid.IsInBounds(coords))
            return true;
        else
            return false;
    }

    public Vector3 ToWorld(int x, int y)
    {
        return grid.GetWorldPosition(x, y);
    }

    private GameObject CreateCellView(Grid<GameObject> grid, int x,int y)
    {
        GameObject gameObject =  GameObject.Instantiate(prefab, grid.GetWorldPosition(x, y), Quaternion.identity, parent.transform);
        gameObject.name += $"{x} {y}";
        gameObject.transform.localScale = new Vector3(grid.GetCellSize(), ySize, grid.GetCellSize());
        instances.Add(gameObject);
        return gameObject;
    }

    public CellState GetCellState(Vector2Int coords)
    {
        return cellStates[coords];
    }
}
