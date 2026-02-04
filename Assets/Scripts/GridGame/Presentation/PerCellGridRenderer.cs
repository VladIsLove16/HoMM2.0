using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx.Toolkit;
using UnityEngine;
using Zenject;

public class PerCellGridRenderer : MonoBehaviour, IGridCellRenderer
{
    [SerializeField] private CellView prefab;
    [SerializeField] private GameObject parent;
    [SerializeField] private GridRenderSettingsSO editorRenderSettings;

    private Grid<CellView> _grid;
    private IGridViewModel _viewModel;
    private IBattleAnimationGate _animationGate;
    private CellViewPool _cellPool;

    private readonly Dictionary<CellState, List<Vector2Int>> _cellStates = new();
    private readonly Dictionary<CellState, HashSet<Vector2Int>> _hiddenStates = new();
    private IReadOnlyDictionary<CellState, CellMaterial> _settingsMaterials = new Dictionary<CellState, CellMaterial>();
    private Dictionary<CellState, CellMaterial> _overrideMaterials;

    private static readonly HashSet<CellState> StatesHiddenWhileLocked = new()
    {
        CellState.activeUnit,
        CellState.reachableCell,
        CellState.enemyReachableCell,
        CellState.hovered,
        CellState.hoveredEnemy,
        CellState.attackTarget,
        CellState.attackTargetBlocked,
        CellState.enemyCell,
        CellState.accessibleRoutePoint,
        CellState.inaccessibleRoutePoint,
        CellState.routeEndAccessible,
        CellState.routeEndBlocked
    };

    [Inject]
    private void Construct(
        IGridViewModel viewModel,
        [InjectOptional] IBattleAnimationGate animationGate = null)
    {
        ApplyRenderSettings(viewModel.RenderSettings);
        _animationGate = animationGate;
        Bind(viewModel);
        UnityLogger.Log("[PerCellGridRenderer] Constructed and bound to ViewModel.");
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
        if (_viewModel != null)
        {
            Unbind(_viewModel);
        }
        Clear();
    }

    private sealed class CellViewPool : ObjectPool<CellView>
    {
        private readonly CellView _prefab;
        private readonly Transform _parent;

        public CellViewPool(CellView prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }

        protected override CellView CreateInstance()
        {
            if (_prefab == null)
                throw new InvalidOperationException("[PerCellGridRenderer] CellView prefab is not assigned for pool.");

            var instance = UnityEngine.Object.Instantiate(_prefab, _parent);
            return instance;
        }
    }

    public void Bind(IGridViewModel viewModel)
    {
        if (_viewModel == viewModel)
            return;

        if (_viewModel != null)
        {
            Unbind(_viewModel);
        }

        _viewModel = viewModel;
        if (_viewModel == null)
            return;

        if (_viewModel.RenderSettings != null)
        {
            ApplyRenderSettings(_viewModel.RenderSettings);
        }

        _viewModel.GridInited += OnGridInited;
        _viewModel.PreviewChanged += OnPreviewChanged;
        _viewModel.PreviewUpdated += OnPreviewUpdated;

        if (_viewModel.Width > 0 && _viewModel.Height > 0)
        {
            BuildGrid(_viewModel.Width, _viewModel.Height);
        }
    }

    public void Unbind(IGridViewModel viewModel)
    {
        if (_viewModel != viewModel || _viewModel == null)
            return;

        _viewModel.GridInited -= OnGridInited;
        _viewModel.PreviewChanged -= OnPreviewChanged;
        _viewModel.PreviewUpdated -= OnPreviewUpdated;
        _viewModel = null;
    }

    public void Clear()
    {
        if (_grid != null)
        {
            foreach (var cell in _grid.GetGridObjects())
            {
                if (cell != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        DestroyImmediate(cell.gameObject);
                    }
                    else
#endif
                    {
                        if (_cellPool != null)
                        {
                            _cellPool.Return(cell);
                        }
                        else
                        {
                            Destroy(cell.gameObject);
                        }
                    }
                }
            }
        }

        _grid = null;
        _cellStates.Clear();
        _hiddenStates.Clear();
    }

    [Button("Render Preview")]
    public void Render()
    {
        var width = Mathf.Max(1, _viewModel?.Width ?? 10);
        var height = Mathf.Max(1, _viewModel?.Height ?? 10);
        BuildGrid(width, height);
    }

    private void OnGridInited(int width, int height)
    {
        BuildGrid(width, height);
    }

    private void BuildGrid(int width, int height)
    {
        var settings = ResolveSettings();
        if (prefab == null)
        {
            Debug.LogError("[PerCellGridRenderer] Cell prefab is not assigned.", this);
            return;
        }

        if (width <= 0 || height <= 0)
        {
            Debug.LogWarning("[PerCellGridRenderer] Grid dimensions must be greater than zero.", this);
            Clear();
            return;
        }

        Clear();

        var parentTransform = parent != null ? parent.transform : transform;
        if (_cellPool == null)
        {
            _cellPool = new CellViewPool(prefab, parentTransform);
        }

        _grid = new Grid<CellView>(
            width,
            height,
            settings.CellSize,
            transform.position,
            settings.CellPadding,
            (grid, x, y) => CreateCellView(grid, x, y, parentTransform));

        ApplyMaterialsToCells();
        ReapplyStates();
    }

    private CellView CreateCellView(Grid<CellView> grid, int x, int y, Transform parentTransform)
    {
        if (_cellPool == null)
        {
            _cellPool = new CellViewPool(prefab, parentTransform);
        }

        var cellView = _cellPool.Rent();
        cellView.transform.SetParent(parentTransform, false);
        cellView.transform.position = grid.GetWorldPosition(x, y);
        cellView.name = $"{prefab.name} {x} {y}";

        var settings = ResolveSettings();
        cellView.transform.localScale = new Vector3(grid.GetCellSize(), settings.CellHeight, grid.GetCellSize());
        cellView.Init(GetCurrentMaterials());

        return cellView;
    }

    private void OnPreviewChanged(PreviewResult result)
    {
        ClearAllStatesInternal();
        foreach (var stateCells in result.ToDictionary())
        {
            AddStatesInternal(stateCells.Value, stateCells.Key);
        }
    }

    private void OnPreviewUpdated(PreviewResult result)
    {
        var dict = result.ToDictionary();
        foreach (var stateCells in dict)
        {
            RemoveStatesInternal(stateCells.Key);
            AddStatesInternal(stateCells.Value, stateCells.Key);
        }
    }

    private void AddStatesInternal(IEnumerable<Vector2Int> coords, CellState state)
    {
        if (coords == null)
            return;

        foreach (var coord in coords)
        {
            AddStateInternal(coord, state);
        }
    }

    private void AddStateInternal(Vector2Int coords, CellState state)
    {
        if (!_cellStates.TryGetValue(state, out var list))
        {
            list = new List<Vector2Int>();
            _cellStates[state] = list;
        }

        if (!list.Contains(coords))
        {
            list.Add(coords);
        }

        RemoveHiddenState(coords, state);

        if (ShouldHideState(state) && IsControlLocked())
        {
            CacheHiddenState(coords, state);
            return;
        }

        if (TryGetCellView(coords, out var cell))
        {
            cell.AddState(state);
        }
    }

    private void RemoveStatesInternal(CellState state)
    {
        if (_cellStates.TryGetValue(state, out var coords))
        {
            foreach (var coord in coords.ToList())
            {
                RemoveStateInternal(coord, state);
            }
            _cellStates.Remove(state);
        }

        if (_hiddenStates.Remove(state))
        {
            // hidden state cache removed
        }
    }

    private void RemoveStateInternal(Vector2Int coords, CellState state)
    {
        if (_cellStates.TryGetValue(state, out var list))
        {
            list.Remove(coords);
        }

        if (TryGetCellView(coords, out var cell))
        {
            cell.RemoveState(state);
        }
    }

    private void ClearAllStatesInternal()
    {
        foreach (var state in _cellStates.Keys.ToList())
        {
            RemoveStatesInternal(state);
        }
        _cellStates.Clear();
        _hiddenStates.Clear();
    }

    private bool TryGetCellView(Vector2Int coords, out CellView cellView)
    {
        cellView = null;
        if (_grid == null || !_grid.IsInBounds(coords))
            return false;

        cellView = _grid.GetGridObject(coords.x, coords.y);
        return cellView != null;
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
                AddStateInternal(coord, entry.State);
            }

            _hiddenStates.Remove(entry.State);
        }
    }

    public void SetPrefab(CellView value) => prefab = value;

    public void SetParent(GameObject newParent) => parent = newParent;

    public void SetRenderSettings(IGridRenderSettings settings) => ApplyRenderSettings(settings);

    public void SetMaterials(List<CellMaterial> materials)
    {
        _overrideMaterials = materials?.ToDictionary(m => m.CellState);
        ApplyMaterialsToCells();
    }

    private void ApplyRenderSettings(IGridRenderSettings settings)
    {
        if (settings == null)
        {
            Debug.LogError("_renderSettings is null. Resolving default");
            settings = ResolveSettings();
            return;
        }
        _settingsMaterials = settings?.Materials?.ToDictionary(m => m.CellState)
                            ?? new Dictionary<CellState, CellMaterial>();
        if (_overrideMaterials == null)
        {
            ApplyMaterialsToCells();
        }
    }

    private void ApplyMaterialsToCells()
    {
        if (_grid == null)
            return;

        foreach (var cell in _grid.GetGridObjects())
        {
            cell?.SetMaterialsDictionary(GetCurrentMaterials());
        }
    }

    private void ReapplyStates()
    {
        if (_grid == null)
            return;

        foreach (var kvp in _cellStates)
        {
            foreach (var coord in kvp.Value)
            {
                if (TryGetCellView(coord, out var cell))
                {
                    cell.AddState(kvp.Key);
                }
            }
        }
    }

    private IReadOnlyDictionary<CellState, CellMaterial> GetCurrentMaterials()
    {
        return _overrideMaterials ?? _settingsMaterials;
    }

    private IGridRenderSettings ResolveSettings()
    {
        if(_viewModel.RenderSettings == null)
        {
            Debug.LogError("_renderSettings is null");
            _viewModel.RenderSettings =  RuntimeGridRenderSettings.Default;
        }
        return _viewModel.RenderSettings;
    }

    private sealed class RuntimeGridRenderSettings : IGridRenderSettings
    {
        public static readonly RuntimeGridRenderSettings Default = new();

        public float CellSize { get; set; } = 1f;
        public float CellHeight { get; set; } = 1f;
        public float CellPadding { get; set; } = 0.1f;
        public IReadOnlyList<CellMaterial> Materials { get; set; } = Array.Empty<CellMaterial>();
    }

    public List<Vector2Int> GetCells(CellState state)
    {
        var result = new List<Vector2Int>();
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

#if UNITY_INCLUDE_TESTS
    public void AddState(Vector2Int coords, CellState state) => AddStateInternal(coords, state);
    public void AddStates(IEnumerable<Vector2Int> coords, CellState state) => AddStatesInternal(coords, state);
    public void RemoveState(Vector2Int coords, CellState state) => RemoveStateInternal(coords, state);
    public void RemoveStates(CellState state) => RemoveStatesInternal(state);
    public void ClearAllStates() => ClearAllStatesInternal();
#endif
}
