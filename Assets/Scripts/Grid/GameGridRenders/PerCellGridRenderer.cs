using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;
[Serializable]
public class CellMaterial
{
    public CellState CellState;
    public Material Material;

}
public class PerCellGridRenderer : IGridCellRenderer, IWorldToCellProvider
{
    private CellView prefab;
    private GameObject parent;
    private float ySize = 1f;

    private Grid<CellView> grid;
    private Dictionary<CellState, CellMaterial> materials = new();
    private Dictionary<CellState, List<Vector2Int>> _cellStates = new();
    private IGridViewModel _vm;
    public PerCellGridRenderer(CellView prefab, GameObject parent, List<CellMaterial> materials)
    {
        this.parent = parent;
        this.prefab = prefab;
        this.materials = materials.ToDictionary(x => x.CellState);
    }

    public void Clear()
    {
        if (grid == null)
            return;
        foreach (CellView child in grid.GetGridObjects())
        {
            GameObject.Destroy(child.gameObject);
        }
        _cellStates.Clear();
    }

    public void Render(int width, int height, float cellSize, Vector3 origin, float padding)
    {
        Clear();
        grid = new Grid<CellView>(width, height, cellSize, origin, padding, CreateCellView);
        

    }

    public void Bind(IGridViewModel viewModel)
    {
        if (_vm != null) Unbind(_vm);
        _vm = viewModel;
        if (_vm == null) return;
        _vm.PreviewChanged += OnreviewChanged;
        Debug.Log("binded");
    }

    public void Unbind(IGridViewModel viewModel)
    {
        if (_vm == null) return;
        _vm.PreviewChanged -= OnreviewChanged;
        _vm = null;
    }

    private void OnreviewChanged(PreviewResult result)
    {
        Dictionary<CellState, List<Vector2Int>> cells = result.ToDictionary();
        foreach (var stateCells in cells)
        {
            RemoveStates(stateCells.Key);
            AddStates(stateCells.Value, stateCells.Key);
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

    public bool ToGridPair(Vector3 position,out KeyValuePair<Vector2Int, Vector2Int> coordPair)
    {
        Vector2Int main = grid.GetXY(position);
        if (!grid.IsInBounds(main))
        {
            coordPair = default;
            return false;
        }
        Vector2Int mainClosestNeighbour = grid.GetClosestNeighbor(position,true);
        coordPair = new(main, mainClosestNeighbour);
        return true;
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
    public void AddStates(List<Vector2Int> points, CellState state)
    {
        foreach (var point in points)
        {
            AddState(point, state);
        }
    }
    public void SetStates(List<Vector2Int> points, CellState state)
    {
        RemoveStates(state);
        AddStates(points, state);
    }
    public void RemoveStates(CellState state)
    {

        if (!_cellStates.ContainsKey(state))
        {
            return;
        }
        var states = _cellStates[state].ToList();
        foreach (var c in states)
        {
            RemoveState(c, state);
        }
    }
    public void ClearAllStates()
    {
        var keys = _cellStates.Keys.ToList();
        foreach (var k in keys)
        {
            RemoveStates(k);
        }
    }
    public void AddState(Vector2Int coords, CellState state)
    {
        if (TryGetCellView(coords, out CellView cellView))
        {
            cellView.AddState(state);
        }
        if(!_cellStates.ContainsKey(state))
            _cellStates[state] = new List<Vector2Int>();
        _cellStates[state].Add(coords);
    }

    public void RemoveState(Vector2Int coords, CellState state)
    {
        if(TryGetCellView(coords,out CellView cellView))
        {
            cellView.RemoveState(state);
        }
        _cellStates[state].Remove(coords);
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
        cellView.Init(materials);
        return cellView;
    }
   
}
