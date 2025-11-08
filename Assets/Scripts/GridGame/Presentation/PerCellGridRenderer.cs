using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using static UnityEngine.UI.Image;
[Serializable]
public class CellMaterial
{
    public CellMaterial(CellState cellState, Material material)
    {
        CellState = cellState;
        Material = material;
    }
    public CellState CellState;
    public Material Material;

}
public class PerCellGridRenderer : MonoBehaviour, IGridCellRenderer, IWorldToCellProvider
{
    [SerializeField] private CellView prefab;
    [SerializeField] private GameObject parent;
    [SerializeField] private float ySize = 1f;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float padding = 0.4f;
    [SerializeField] List<CellMaterial> Materials;
    [Inject] private IGridViewModel _vm;
    [Inject(Optional = true)] private IBattleAnimationGate _animationGate;
    private Grid<CellView> grid;
    private Dictionary<CellState, CellMaterial> materialsDict = new();
    private Dictionary<CellState, List<Vector2Int>> _cellStates = new();
    private readonly Dictionary<CellState, HashSet<Vector2Int>> _hiddenStates = new();
    private static readonly HashSet<CellState> StatesHiddenWhileLocked = new()
    {
        CellState.reachableCell,
        CellState.accessibleRoutePoint,
        CellState.inaccessibleRoutePoint
    };
    private void Awake()
    {
        if (Materials == null)
        {
            materialsDict = new Dictionary<CellState, CellMaterial>();
        }
        else
        {
            materialsDict = Materials.ToDictionary(x => x.CellState);
        }
    }

    [Inject]
    private void Initialize(IGridViewModel viewModel)
    {
        Bind(viewModel);
    }

    private void OnEnable()
    {
        if (_animationGate != null)
        {
            _animationGate.LockStateChanged += OnAnimationLockStateChanged;
            if (_animationGate.IsLocked)
            {
                SuppressVisibleStates();
            }
        }
    }

    private void OnDisable()
    {
        if (_animationGate != null)
        {
            _animationGate.LockStateChanged -= OnAnimationLockStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (_vm != null)
        {
            Unbind(_vm);
        }
    }
    public void Clear()
    {
        if (grid != null)
        {
            foreach (CellView child in grid.GetGridObjects())
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        _cellStates.Clear();
        _hiddenStates.Clear();
    }
    [Button]
    public void Render()
    {
        Render(10, 10, cellSize, transform.position, padding);
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
        _vm.PreviewChanged += OnPreviewChanged;
        _vm.PreviewUpdated += OnPreviewUpdated;
        _vm.GridInited+=OnVM_GridInited;
    }

    public void Unbind(IGridViewModel viewModel)
    {
        if (_vm == null) return;
        _vm.PreviewChanged -= OnPreviewChanged;
        _vm.GridInited -= OnVM_GridInited;
        _vm = null;
    }
    private void OnVM_GridInited(int x,int y)
    {
        Render(x, y, cellSize,transform.position, padding);
    }
    private void OnPreviewUpdated(PreviewResult result)
    {
        string previewString = string.Empty;
        Dictionary<CellState, List<Vector2Int>> cells = result.ToDictionary();
        foreach (var stateCells in cells)
        {
            RemoveStates(stateCells.Key);
            AddStates(stateCells.Value, stateCells.Key);
            string coordsString = string.Empty;
            foreach(var  cell in stateCells.Value)
            {
                coordsString += cell + " "; 
            }
            previewString += stateCells.Key + " " + coordsString;
        }
    }
    private void OnPreviewChanged(PreviewResult result)
    {
        string previewString = string.Empty;
        Dictionary<CellState, List<Vector2Int>> cells = result.ToDictionary();
        ClearAllStates();
        foreach (var stateCells in cells)
        {
            AddStates(stateCells.Value, stateCells.Key);
            string coordsString = string.Empty;
            foreach(var  cell in stateCells.Value)
            {
                coordsString += cell + " "; 
            }
            previewString += stateCells.Key + " " + coordsString;
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
        if(grid == null)
        {
            Debug.LogError("Grid is null");
            return Vector3.zero;
        }
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
        HashSet<Vector2Int> coords = new HashSet<Vector2Int>();
        if (_cellStates.TryGetValue(state, out var visible))
        {
            coords.UnionWith(visible);
        }
        if (_hiddenStates.TryGetValue(state, out var hidden))
        {
            coords.UnionWith(hidden);
        }

        if (coords.Count == 0)
        {
            return;
        }

        foreach (var c in coords)
        {
            RemoveState(c, state);
        }
    }
    public void ClearAllStates()
    {
        var keys = _cellStates.Keys
            .Concat(_hiddenStates.Keys)
            .Distinct()
            .ToList();

        foreach (var k in keys)
        {
            RemoveStates(k);
        }
    }
    public void AddState(Vector2Int coords, CellState state)
    {
        if (ShouldHideState(state) && IsControlLocked())
        {
            CacheHiddenState(coords, state);
            return;
        }

        if (TryGetCellView(coords, out CellView cellView))
        {
            cellView.AddState(state);
        }
        else
            throw new Exception(" no value for " + state);

        if (!_cellStates.ContainsKey(state))
        {
            _cellStates[state] = new List<Vector2Int>();
        }
        if (!_cellStates[state].Contains(coords))
        {
            _cellStates[state].Add(coords);
        }

        RemoveHiddenState(coords, state);
    }

    public void RemoveState(Vector2Int coords, CellState state)
    {
        bool wasVisible = _cellStates.TryGetValue(state, out var visibleList) && visibleList.Contains(coords);
        if (wasVisible && TryGetCellView(coords, out CellView cellView))
        {
            cellView.RemoveState(state);
        }

        if (wasVisible)
        {
            visibleList.Remove(coords);
            if (visibleList.Count == 0)
            {
                _cellStates.Remove(state);
            }
        }

        if (_hiddenStates.TryGetValue(state, out var hidden))
        {
            hidden.Remove(coords);
            if (hidden.Count == 0)
            {
                _hiddenStates.Remove(state);
            }
        }
    }

    public List<Vector2Int> GetCells( CellState state)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        if (_cellStates.TryGetValue(state, out var visible))
        {
            result.AddRange(visible);
        }
        if (_hiddenStates.TryGetValue(state, out var hidden))
        {
            result.AddRange(hidden);
        }

        return result;
    }

    private bool ShouldHideState(CellState state) => StatesHiddenWhileLocked.Contains(state);

    private bool IsControlLocked() => _animationGate != null && _animationGate.IsLocked;

    private void CacheHiddenState(Vector2Int coords, CellState state)
    {
        if (!_hiddenStates.TryGetValue(state, out var set))
        {
            set = new HashSet<Vector2Int>();
            _hiddenStates[state] = set;
        }

        set.Add(coords);
    }

    private void RemoveHiddenState(Vector2Int coords, CellState state)
    {
        if (_hiddenStates.TryGetValue(state, out var set))
        {
            set.Remove(coords);
            if (set.Count == 0)
            {
                _hiddenStates.Remove(state);
            }
        }
    }

    private void OnAnimationLockStateChanged(bool isLocked)
    {
        if (isLocked)
        {
            SuppressVisibleStates();
        }
        else
        {
            RestoreHiddenStates();
        }
    }

    private void SuppressVisibleStates()
    {
        foreach (var state in StatesHiddenWhileLocked)
        {
            if (!_cellStates.TryGetValue(state, out var coords))
                continue;

            foreach (var coord in coords.ToList())
            {
                CacheHiddenState(coord, state);
                if (TryGetCellView(coord, out var cellView))
                {
                    cellView.RemoveState(state);
                }
            }

            _cellStates.Remove(state);
        }
    }

    private void RestoreHiddenStates()
    {
        var snapshot = _hiddenStates
            .Where(kvp => StatesHiddenWhileLocked.Contains(kvp.Key))
            .Select(kvp => (State: kvp.Key, Coords: kvp.Value.ToList()))
            .ToList();

        foreach (var entry in snapshot)
        {
            foreach (var coord in entry.Coords)
            {
                AddState(coord, entry.State);
            }

            _hiddenStates.Remove(entry.State);
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
        cellView.Init(materialsDict);
        return cellView;
    }

    // Public setters to allow tests and other runtime code to inject dependencies without reflection
    public void SetPrefab(CellView p)
    {
        prefab = p;
    }

    public void SetParent(GameObject p)
    {
        parent = p;
    }

    public void SetMaterials(List<CellMaterial> materials)
    {
        Materials = materials;
        // reinitialize materials dictionary if Awake already ran
        if (materialsDict != null)
        {
            materialsDict = Materials.ToDictionary(x => x.CellState);
        }
    }
   
}
