using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.Image;
[Serializable]
public class CellMaterials
{
    public CellState CellState;
    public Material Material;

}
public class PerCellGridRenderer : IGridCellRenderer
{
    List<CellView> instances = new List<CellView>();
    CellView prefab;
    GameObject parent;
    float ySize = 1f;
    Grid<CellView> grid;
    private Dictionary<CellState,CellMaterials> materials = new ();
    private Dictionary<Vector2Int,CellState> cellStates = new ();
    private Dictionary<Vector2Int,CellState> previousStates = new ();
    public PerCellGridRenderer(CellView prefab, GameObject parent, List<CellMaterials> materials)
    {
        this.parent = parent;
        this.prefab = prefab;
        this.materials = materials.ToDictionary(x => x.CellState);
    }

    public void Clear()
    {
        foreach (CellView child in instances)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void Render(int width, int height, float cellSize,Vector3 origin, float padding)
    {
        Clear();
        grid = new Grid<CellView>(width, height, cellSize, origin, padding, CreateCellView);
        for(int i = 0; i< width; i++)
        {
            for (int j = 0; j < width; j++)
            {
                previousStates[new Vector2Int(i,j)] = CellState.normal;
            }
        }
        
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

    public CellState[] GetCellStates(Vector2Int coords)
    {
       if( TryGetCellView(coords, out var cell))
        {
           return cell.GetStates();
        }
       else
            return null;
    }

    public void AddState(Vector2Int coords, CellState state)
    {
        if (TryGetCellView(coords, out CellView cellView))
        {
            cellView.AddState(state);
        }
    }

    public void RemoveState(Vector2Int coords, CellState state)
    {
        if(TryGetCellView(coords,out CellView cellView))
        {
            cellView.RemoveState(state);
        }
    }
    private bool TryGetCellView(Vector2Int coords, out CellView cellView)
    {
        if(!grid.IsInBounds(coords))
        {
            cellView = null;
            return false;
        }
        cellView = grid.GetGridObject(coords.x, coords.y);
        return true;
    }

    private CellView CreateCellView(Grid<CellView> grid, int x,int y)
    {
        CellView cellView =  GameObject.Instantiate(prefab, grid.GetWorldPosition(x, y), Quaternion.identity, parent.transform);
        cellView.name += $"{x} {y}";
        cellView.transform.localScale = new Vector3(grid.GetCellSize(), ySize, grid.GetCellSize());
        instances.Add(cellView);
        cellView.Init(materials);
        return cellView;
    }
   
}
