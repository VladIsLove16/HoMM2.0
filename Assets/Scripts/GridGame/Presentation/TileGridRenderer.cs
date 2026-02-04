using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TileGridRenderer : MonoBehaviour, IGridCellRenderer
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Color lineColor = Color.white;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private GameObject focusIndicatorPrefab;
    [SerializeField] private Transform focusIndicatorParent;
    [SerializeField] private bool createSurfaceCollider = true;
    [SerializeField] private float colliderThickness = 0.05f;
    [SerializeField] private float highlightYOffset = 0.02f;
    [SerializeField] private float routeInset = 0.15f;
    [SerializeField] private float routeLineThickness = 0.06f;
    [SerializeField] private float borderInset = 0.08f;
    [SerializeField] private float borderThickness = 0.08f;
    [SerializeField] private float targetInset = 0.2f;
    [SerializeField] private float routeEndInset = 0.2f;
    [SerializeField] private float routeLineYOffset = 0.004f;
    [SerializeField] private float innerFillYOffset = 0.01f;
    [SerializeField] private float hoverBorderYOffset = 0.012f;
    [SerializeField] private float hoverInset = 0.22f;
    [SerializeField] private float hoverBorderThickness = 0.05f;
    [SerializeField] private float selectedInset = 0.12f;
    [SerializeField] private float selectedBorderThickness = 0.06f;
    [SerializeField] private GridRenderSettingsSO editorRenderSettings;
    [SerializeField] private Material fallbackHoverMaterial;
    [SerializeField] private float routePulseSpeed = 2.0f;
    [SerializeField] private float routePulseAmplitude = 0.25f;

    private Mesh _gridMesh;
    private IGridViewModel _viewModel;
    private IWorldToCellProvider _coordinateMapper;
    private IBattleAnimationGate _animationGate;
    private GameObject _focusIndicator;
    private BoxCollider _surfaceCollider;
    private bool _ownsCollider;

    private IGridRenderSettings _renderSettings;
    private readonly Dictionary<CellState, CellMaterial> _materials = new();
    private readonly Dictionary<CellState, HighlightLayer> _highlightLayers = new();
    private readonly Dictionary<CellState, List<Vector2Int>> _cellStates = new();
    private readonly Dictionary<CellState, Color> _baseColors = new();
    private MaterialPropertyBlock _routePulseBlock;

    private Vector3 _gridOriginWorld;
    private float _cellSpacing = 1f;

    private static readonly HashSet<CellState> StatesHiddenWhileLocked = new()
    {
        CellState.activeUnit,
        CellState.reachableCell,
        CellState.enemyReachableCell,
        CellState.hovered,
        CellState.attackTarget,
        CellState.attackTargetBlocked,
        CellState.enemyCell,
        CellState.hoveredEnemy,
        CellState.accessibleRoutePoint,
        CellState.inaccessibleRoutePoint,
        CellState.routeEndAccessible,
        CellState.routeEndBlocked
    };

    private sealed class HighlightLayer
    {
        public GameObject GameObject { get; }
        public MeshFilter Filter { get; }
        public MeshRenderer Renderer { get; }
        public Mesh Mesh { get; }

        public HighlightLayer(GameObject go, MeshFilter filter, MeshRenderer renderer)
        {
            GameObject = go;
            Filter = filter;
            Renderer = renderer;
            Mesh = new Mesh { name = $"{go.name}_Mesh" };
            Filter.sharedMesh = Mesh;
        }
    }

    [Inject]
    private void Construct(IGridViewModel viewModel, IWorldToCellProvider mapper, [InjectOptional] IBattleAnimationGate animationGate = null)
    {
        _coordinateMapper = mapper;
        _animationGate = animationGate;
        SetRenderSettings(viewModel?.RenderSettings ?? editorRenderSettings);
        Bind(viewModel);
    }

    private void OnEnable()
    {
        if (_animationGate != null)
        {
            _animationGate.LockStateChanged += OnAnimationLockStateChanged;
        }
    }

    private void OnDisable()
    {
        if (_animationGate != null)
        {
            _animationGate.LockStateChanged -= OnAnimationLockStateChanged;
        }
    }

    private void Update()
    {
        UpdateRoutePulse();
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

        SetRenderSettings(_viewModel.RenderSettings ?? editorRenderSettings);

        _viewModel.GridInited += OnGridInited;
        _viewModel.PreviewChanged += OnPreviewChanged;
        _viewModel.PreviewUpdated += OnPreviewUpdated;

        if (_viewModel.Width > 0 && _viewModel.Height > 0)
        {
            BuildMesh(_viewModel.Width, _viewModel.Height);
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
        if (_gridMesh != null)
        {
            _gridMesh.Clear();
        }

        if (meshFilter != null)
        {
            meshFilter.sharedMesh = null;
        }

        if (_focusIndicator != null)
        {
            Destroy(_focusIndicator);
            _focusIndicator = null;
        }

        if (_surfaceCollider != null)
        {
            _surfaceCollider.enabled = false;
        }

        ClearAllStates();
    }

    private void SetRenderSettings(IGridRenderSettings settings)
    {
        _renderSettings = settings ?? editorRenderSettings;
        if ((_renderSettings == null || _renderSettings.Materials == null || _renderSettings.Materials.Count == 0) &&
            editorRenderSettings != null)
        {
            Debug.LogWarning("[TileGridRenderer] RenderSettings had no materials. Falling back to editorRenderSettings.", this);
            _renderSettings = editorRenderSettings;
        }
        _materials.Clear();
        _baseColors.Clear();

        if (_renderSettings?.Materials != null)
        {
            foreach (var mat in _renderSettings.Materials)
            {
                if (mat == null || mat.Material == null)
                    continue;
                _materials[mat.CellState] = mat;
                var material = mat.Material;
                if (material != null)
                {
                    _baseColors[mat.CellState] = ResolveBaseColor(material);
                }
            }
        }

        RebuildHighlightLayers();
        ClearAllStates();
    }

    private void RebuildHighlightLayers()
    {
        foreach (var layer in _highlightLayers.Values)
        {
            if (layer.GameObject != null)
            {
                Destroy(layer.GameObject);
            }
            if (layer.Mesh != null)
            {
                Destroy(layer.Mesh);
            }
        }
        _highlightLayers.Clear();

        foreach (var kvp in _materials)
        {
            TryCreateHighlightLayer(kvp.Key, kvp.Value?.Material);
        }

        if (!_highlightLayers.ContainsKey(CellState.hovered))
        {
            Debug.LogWarning("[TileGridRenderer] Hover material is missing in GridRenderSettings and no fallback is provided.", this);
        }

        RefreshAllHighlightLayers();
    }

    private bool TryCreateHighlightLayer(CellState state, Material material)
    {
        if (material == null)
        {
            return false;
        }

        var go = new GameObject($"TileHighlight_{state}");
        go.layer = gameObject.layer;
        go.transform.SetParent(transform, false);
        var filter = go.AddComponent<MeshFilter>();
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

        _highlightLayers[state] = new HighlightLayer(go, filter, renderer);
        if (!_baseColors.ContainsKey(state))
        {
            _baseColors[state] = ResolveBaseColor(material);
        }
        return true;
    }

    private void OnGridInited(int width, int height)
    {
        BuildMesh(width, height);
    }

    private void BuildMesh(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            Clear();
            return;
        }

        var settings = _renderSettings;
        _cellSpacing = (settings?.CellSize ?? 1f) + (settings?.CellPadding ?? 0f);
        _gridOriginWorld = transform.position - new Vector3(_cellSpacing * 0.5f, 0f, _cellSpacing * 0.5f);

        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        if (_gridMesh == null)
        {
            _gridMesh = new Mesh { name = "TileGridMesh" };
            _gridMesh.indexFormat = IndexFormat.UInt32;
        }
        else
        {
            _gridMesh.Clear();
        }

        var lines = new List<Vector3>();

        for (int x = 0; x <= width; x++)
        {
            var start = _gridOriginWorld + new Vector3(x * _cellSpacing, 0f, 0f);
            var end = start + new Vector3(0f, 0f, height * _cellSpacing);
            lines.Add(start);
            lines.Add(end);
        }

        for (int y = 0; y <= height; y++)
        {
            var start = _gridOriginWorld + new Vector3(0f, 0f, y * _cellSpacing);
            var end = start + new Vector3(width * _cellSpacing, 0f, 0f);
            lines.Add(start);
            lines.Add(end);
        }

        _gridMesh.SetVertices(lines);
        var indices = new int[lines.Count];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = i;
        }
        _gridMesh.SetIndices(indices, MeshTopology.Lines, 0);

        meshFilter.sharedMesh = _gridMesh;
        ApplyLineAppearance();
        UpdateSurfaceCollider(width, height);
        RefreshAllHighlightLayers();
    }

    private void ApplyLineAppearance()
    {
        if (meshRenderer == null)
            return;

        var material = meshRenderer.sharedMaterial;
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
            meshRenderer.sharedMaterial = material;
        }

        material.color = lineColor;
        material.SetFloat("_LineWidth", lineWidth);
    }

    private void OnPreviewChanged(PreviewResult result)
    {
        ClearAllStates();
        if (result == null)
        {
            UpdateFocusIndicator(null);
            return;
        }

        var dict = result.ToDictionary();
        if (dict == null)
            return;

        var changed = new HashSet<CellState>();
        foreach (var stateCells in dict)
        {
            if (!TrySetState(stateCells.Key, stateCells.Value))
                continue;
            changed.Add(stateCells.Key);
        }

        foreach (var state in changed)
        {
            UpdateHighlightLayer(state);
        }

        UpdateFocusIndicator(GetHoveredCell(dict));
    }

    private void OnPreviewUpdated(PreviewResult result)
    {
        if (result == null)
        {
            UpdateFocusIndicator(null);
            return;
        }

        var dict = result.ToDictionary();
        if (dict == null)
            return;

        var changed = new HashSet<CellState>();
        foreach (var stateCells in dict)
        {
            if (!TrySetState(stateCells.Key, stateCells.Value))
                continue;
            changed.Add(stateCells.Key);
        }

        foreach (var state in changed)
        {
            UpdateHighlightLayer(state);
        }

        UpdateFocusIndicator(GetHoveredCell(dict));
    }

    private Vector2Int? GetHoveredCell(Dictionary<CellState, List<Vector2Int>> data)
    {
        if (data != null && data.TryGetValue(CellState.hovered, out var cells) && cells != null && cells.Count > 0)
        {
            return cells[0];
        }
        return null;
    }

    private bool TrySetState(CellState state, IEnumerable<Vector2Int> coords)
    {
        if (!_materials.ContainsKey(state) && state != CellState.hovered)
        {
            return false;
        }

        var list = coords?.Distinct().ToList();
        if (list == null || list.Count == 0)
        {
            _cellStates.Remove(state);
        }
        else
        {
            _cellStates[state] = list;
        }

        return true;
    }

    private void ClearAllStates()
    {
        if (_cellStates.Count == 0)
            return;

        _cellStates.Clear();
        RefreshAllHighlightLayers();
    }

    private void RefreshAllHighlightLayers()
    {
        foreach (var state in _highlightLayers.Keys)
        {
            UpdateHighlightLayer(state);
        }
    }

    private void UpdateHighlightLayer(CellState state)
    {
        if (!_highlightLayers.TryGetValue(state, out var layer))
            return;

        var mesh = layer.Mesh;
        mesh.Clear();

        if (IsControlLocked() && ShouldHideState(state))
        {
            return;
        }

        if (!_cellStates.TryGetValue(state, out var cells) || cells == null || cells.Count == 0)
        {
            return;
        }

        if (state == CellState.reachableCell || state == CellState.enemyReachableCell)
        {
            var filtered = FilterOverlaps(state, cells);
            if (filtered.Count > 0)
            {
                BuildFillWithOuterBorder(mesh, filtered);
            }
        }
        else if (state == CellState.activeUnit)
        {
            BuildBorderMesh(mesh, cells, selectedInset, selectedBorderThickness);
        }
        else if (state == CellState.hovered)
        {
            UpdateHoverMaterial(layer, cells);
            BuildBorderMesh(mesh, cells, hoverInset, hoverBorderThickness, highlightYOffset + hoverBorderYOffset);
        }
        else if (state == CellState.hoveredEnemy)
        {
            BuildBorderMesh(mesh, cells, hoverInset, hoverBorderThickness, highlightYOffset + hoverBorderYOffset);
        }
        else if (state == CellState.attackTarget || state == CellState.attackTargetBlocked || state == CellState.enemyCell)
        {
            BuildInsetFillMesh(mesh, cells, targetInset, highlightYOffset + innerFillYOffset);
        }
        else if (state == CellState.routeEndAccessible || state == CellState.routeEndBlocked)
        {
            BuildInsetFillMesh(mesh, cells, routeEndInset, highlightYOffset + innerFillYOffset);
        }
        else if (state == CellState.accessibleRoutePoint)
        {
            BuildRouteLineMesh(mesh, cells, null, routeLineYOffset);
        }
        else if (state == CellState.inaccessibleRoutePoint)
        {
            BuildRouteLineMesh(mesh, cells, GetStateSet(CellState.accessibleRoutePoint), routeLineYOffset + 0.001f);
        }
        else
        {
            BuildFilledMesh(mesh, cells);
        }
    }

    private void OnAnimationLockStateChanged(bool isLocked)
    {
        RefreshAllHighlightLayers();
    }

    private bool IsControlLocked() => _animationGate != null && _animationGate.IsLocked;

    private bool ShouldHideState(CellState state) => StatesHiddenWhileLocked.Contains(state);

    private void BuildFilledMesh(Mesh mesh, List<Vector2Int> cells)
    {
        var quadCount = cells.Count;
        var vertices = new Vector3[quadCount * 4];
        var uvs = new Vector2[quadCount * 4];
        var triangles = new int[quadCount * 6];

        for (int i = 0; i < quadCount; i++)
        {
            var cell = cells[i];
            var start = _gridOriginWorld + new Vector3(cell.x * _cellSpacing, 0f, cell.y * _cellSpacing);
            var vi = i * 4;
            vertices[vi] = start + new Vector3(0f, highlightYOffset, 0f);
            vertices[vi + 1] = start + new Vector3(_cellSpacing, highlightYOffset, 0f);
            vertices[vi + 2] = start + new Vector3(_cellSpacing, highlightYOffset, _cellSpacing);
            vertices[vi + 3] = start + new Vector3(0f, highlightYOffset, _cellSpacing);

            uvs[vi] = new Vector2(0f, 0f);
            uvs[vi + 1] = new Vector2(1f, 0f);
            uvs[vi + 2] = new Vector2(1f, 1f);
            uvs[vi + 3] = new Vector2(0f, 1f);

            var ti = i * 6;
            triangles[ti] = vi;
            triangles[ti + 1] = vi + 2;
            triangles[ti + 2] = vi + 1;
            triangles[ti + 3] = vi;
            triangles[ti + 4] = vi + 3;
            triangles[ti + 5] = vi + 2;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    private void BuildRouteLineMesh(Mesh mesh, List<Vector2Int> cells, HashSet<Vector2Int> neighborSet, float yOffset)
    {
        var vertices = new List<Vector3>(cells.Count * 8);
        var uvs = new List<Vector2>(cells.Count * 8);
        var triangles = new List<int>(cells.Count * 12);

        if (cells.Count == 0)
        {
            mesh.Clear();
            return;
        }

        float thickness = Mathf.Max(routeLineThickness, _cellSpacing * 0.04f);
        float inset = 0f;

        if (neighborSet != null && cells.Count > 0)
        {
            if (TryGetAdjacentCell(cells[0], neighborSet, out var prev))
            {
                var y = highlightYOffset + yOffset;
                AddLineSegment(vertices, uvs, triangles,
                    CellCenter(prev) + Vector3.up * y,
                    CellCenter(cells[0]) + Vector3.up * y,
                    thickness, inset);
            }
        }

        for (int i = 1; i < cells.Count; i++)
        {
            var y = highlightYOffset + yOffset;
            var from = CellCenter(cells[i - 1]) + Vector3.up * y;
            var to = CellCenter(cells[i]) + Vector3.up * y;
            AddLineSegment(vertices, uvs, triangles, from, to, thickness, inset);
        }

        if (neighborSet != null && cells.Count > 0)
        {
            if (TryGetAdjacentCell(cells[^1], neighborSet, out var next))
            {
                var y = highlightYOffset + yOffset;
                AddLineSegment(vertices, uvs, triangles,
                    CellCenter(cells[^1]) + Vector3.up * y,
                    CellCenter(next) + Vector3.up * y,
                    thickness, inset);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
    }

    private static void AddQuad(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, Vector3 min, Vector3 max)
    {
        int index = vertices.Count;
        vertices.Add(new Vector3(min.x, min.y, min.z));
        vertices.Add(new Vector3(max.x, max.y, min.z));
        vertices.Add(new Vector3(max.x, max.y, max.z));
        vertices.Add(new Vector3(min.x, min.y, max.z));

        uvs.Add(new Vector2(0f, 0f));
        uvs.Add(new Vector2(1f, 0f));
        uvs.Add(new Vector2(1f, 1f));
        uvs.Add(new Vector2(0f, 1f));

        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 1);
        triangles.Add(index);
        triangles.Add(index + 3);
        triangles.Add(index + 2);
    }

    private void BuildBorderMesh(Mesh mesh, List<Vector2Int> cells)
    {
        BuildBorderMesh(mesh, cells, borderInset, borderThickness);
    }

    private void BuildBorderMesh(Mesh mesh, List<Vector2Int> cells, float inset, float thickness)
    {
        BuildBorderMesh(mesh, cells, inset, thickness, highlightYOffset);
    }

    private void BuildBorderMesh(Mesh mesh, List<Vector2Int> cells, float inset, float thickness, float yOffset)
    {
        var set = new HashSet<Vector2Int>(cells);
        var vertices = new List<Vector3>(cells.Count * 8);
        var uvs = new List<Vector2>(cells.Count * 8);
        var triangles = new List<int>(cells.Count * 12);

        float clampedInset = Mathf.Clamp(inset, 0f, _cellSpacing * 0.25f);

        for (int i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            var start = _gridOriginWorld + new Vector3(cell.x * _cellSpacing, 0f, cell.y * _cellSpacing);

            float minX = start.x + clampedInset;
            float maxX = start.x + _cellSpacing - clampedInset;
            float minZ = start.z + clampedInset;
            float maxZ = start.z + _cellSpacing - clampedInset;

            if (maxX <= minX || maxZ <= minZ)
                continue;

            float maxThickness = Mathf.Max(0.001f, Mathf.Min(maxX - minX, maxZ - minZ) * 0.5f);
            float clampedThickness = Mathf.Clamp(thickness, Mathf.Max(0.01f, _cellSpacing * 0.02f), maxThickness);
            float y = yOffset;

            if (!set.Contains(new Vector2Int(cell.x, cell.y - 1)))
            {
                AddQuad(vertices, uvs, triangles, new Vector3(minX, y, minZ), new Vector3(maxX, y, minZ + clampedThickness));
            }
            if (!set.Contains(new Vector2Int(cell.x, cell.y + 1)))
            {
                AddQuad(vertices, uvs, triangles, new Vector3(minX, y, maxZ - clampedThickness), new Vector3(maxX, y, maxZ));
            }
            if (!set.Contains(new Vector2Int(cell.x - 1, cell.y)))
            {
                AddQuad(vertices, uvs, triangles, new Vector3(minX, y, minZ), new Vector3(minX + clampedThickness, y, maxZ));
            }
            if (!set.Contains(new Vector2Int(cell.x + 1, cell.y)))
            {
                AddQuad(vertices, uvs, triangles, new Vector3(maxX - clampedThickness, y, minZ), new Vector3(maxX, y, maxZ));
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
    }

    private void BuildFillWithOuterBorder(Mesh mesh, List<Vector2Int> cells)
    {
        var set = new HashSet<Vector2Int>(cells);
        var vertices = new List<Vector3>(cells.Count * 12);
        var uvs = new List<Vector2>(cells.Count * 12);
        var triangles = new List<int>(cells.Count * 18);

        float inset = Mathf.Clamp(borderInset, 0f, _cellSpacing * 0.25f);

        foreach (var cell in cells)
        {
            var start = _gridOriginWorld + new Vector3(cell.x * _cellSpacing, 0f, cell.y * _cellSpacing);
            float minX = start.x + inset;
            float maxX = start.x + _cellSpacing - inset;
            float minZ = start.z + inset;
            float maxZ = start.z + _cellSpacing - inset;

            if (maxX <= minX || maxZ <= minZ)
                continue;

            AddQuad(vertices, uvs, triangles, new Vector3(minX, highlightYOffset, minZ), new Vector3(maxX, highlightYOffset, maxZ));

            float maxThickness = Mathf.Max(0.001f, Mathf.Min(maxX - minX, maxZ - minZ) * 0.5f);
            float thickness = Mathf.Clamp(borderThickness, Mathf.Max(0.01f, _cellSpacing * 0.02f), maxThickness);
            float y = highlightYOffset;

            if (!set.Contains(new Vector2Int(cell.x, cell.y - 1)))
            {
                AddQuad(vertices, uvs, triangles, new Vector3(minX, y, minZ), new Vector3(maxX, y, minZ + thickness));
            }
            if (!set.Contains(new Vector2Int(cell.x, cell.y + 1)))
            {
                AddQuad(vertices, uvs, triangles, new Vector3(minX, y, maxZ - thickness), new Vector3(maxX, y, maxZ));
            }
            if (!set.Contains(new Vector2Int(cell.x - 1, cell.y)))
            {
                AddQuad(vertices, uvs, triangles, new Vector3(minX, y, minZ), new Vector3(minX + thickness, y, maxZ));
            }
            if (!set.Contains(new Vector2Int(cell.x + 1, cell.y)))
            {
                AddQuad(vertices, uvs, triangles, new Vector3(maxX - thickness, y, minZ), new Vector3(maxX, y, maxZ));
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
    }

    private void BuildInsetFillMesh(Mesh mesh, List<Vector2Int> cells, float inset)
    {
        BuildInsetFillMesh(mesh, cells, inset, highlightYOffset);
    }

    private void BuildInsetFillMesh(Mesh mesh, List<Vector2Int> cells, float inset, float yOffset)
    {
        var quadCount = cells.Count;
        var vertices = new Vector3[quadCount * 4];
        var uvs = new Vector2[quadCount * 4];
        var triangles = new int[quadCount * 6];

        float clampedInset = Mathf.Max(0f, inset);

        for (int i = 0; i < quadCount; i++)
        {
            var cell = cells[i];
            var start = _gridOriginWorld + new Vector3(cell.x * _cellSpacing, 0f, cell.y * _cellSpacing);
            float minX = start.x + clampedInset;
            float maxX = start.x + _cellSpacing - clampedInset;
            float minZ = start.z + clampedInset;
            float maxZ = start.z + _cellSpacing - clampedInset;

            if (maxX <= minX || maxZ <= minZ)
                continue;

            var vi = i * 4;
            vertices[vi] = new Vector3(minX, yOffset, minZ);
            vertices[vi + 1] = new Vector3(maxX, yOffset, minZ);
            vertices[vi + 2] = new Vector3(maxX, yOffset, maxZ);
            vertices[vi + 3] = new Vector3(minX, yOffset, maxZ);

            uvs[vi] = new Vector2(0f, 0f);
            uvs[vi + 1] = new Vector2(1f, 0f);
            uvs[vi + 2] = new Vector2(1f, 1f);
            uvs[vi + 3] = new Vector2(0f, 1f);

            var ti = i * 6;
            triangles[ti] = vi;
            triangles[ti + 1] = vi + 2;
            triangles[ti + 2] = vi + 1;
            triangles[ti + 3] = vi;
            triangles[ti + 4] = vi + 3;
            triangles[ti + 5] = vi + 2;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    private void AddLineSegment(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, Vector3 from, Vector3 to, float thickness, float inset)
    {
        var dir = (to - from);
        var length = dir.magnitude;
        if (length <= 0.0001f)
            return;

        var dirNorm = dir / length;
        var trimmedFrom = from + dirNorm * inset;
        var trimmedTo = to - dirNorm * inset;
        var perp = new Vector3(-dirNorm.z, 0f, dirNorm.x) * (thickness * 0.5f);
        var v0 = trimmedFrom - perp;
        var v1 = trimmedFrom + perp;
        var v2 = trimmedTo + perp;
        var v3 = trimmedTo - perp;

        int index = vertices.Count;
        vertices.Add(v0);
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        uvs.Add(new Vector2(0f, 0f));
        uvs.Add(new Vector2(1f, 0f));
        uvs.Add(new Vector2(1f, 1f));
        uvs.Add(new Vector2(0f, 1f));

        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 1);
        triangles.Add(index);
        triangles.Add(index + 3);
        triangles.Add(index + 2);

        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 3);
    }

    private Vector3 CellCenter(Vector2Int cell)
    {
        return _gridOriginWorld + new Vector3((cell.x + 0.5f) * _cellSpacing, highlightYOffset, (cell.y + 0.5f) * _cellSpacing);
    }

    private static Color ResolveBaseColor(Material material)
    {
        if (material.HasProperty("_BaseColor"))
            return material.GetColor("_BaseColor");
        if (material.HasProperty("_Color"))
            return material.GetColor("_Color");
        return material.color;
    }

    private void UpdateHoverMaterial(HighlightLayer layer, List<Vector2Int> cells)
    {
        if (layer == null || layer.Renderer == null)
            return;

        if (cells == null || cells.Count == 0)
            return;

        var cell = cells[0];
        Material targetMaterial = null;
        if (_cellStates.TryGetValue(CellState.reachableCell, out var reachable) && reachable.Contains(cell))
        {
            _materials.TryGetValue(CellState.reachableCell, out var mat);
            targetMaterial = mat?.Material;
        }
        else if (_materials.TryGetValue(CellState.inaccessibleRoutePoint, out var mat))
        {
            targetMaterial = mat?.Material;
        }

        if (targetMaterial == null && fallbackHoverMaterial != null)
        {
            targetMaterial = fallbackHoverMaterial;
        }

        if (targetMaterial != null && layer.Renderer.sharedMaterial != targetMaterial)
        {
            layer.Renderer.sharedMaterial = targetMaterial;
        }
    }

    private HashSet<Vector2Int> GetStateSet(CellState state)
    {
        return _cellStates.TryGetValue(state, out var list) && list != null
            ? new HashSet<Vector2Int>(list)
            : null;
    }

    private List<Vector2Int> FilterOverlaps(CellState state, List<Vector2Int> cells)
    {
        if (cells == null || cells.Count == 0)
            return new List<Vector2Int>();

        if (state == CellState.reachableCell &&
            _cellStates.TryGetValue(CellState.enemyReachableCell, out var enemyReachable) &&
            enemyReachable.Count > 0)
        {
            return cells.FindAll(cell => !enemyReachable.Contains(cell));
        }

        return cells;
    }

    private static bool TryGetAdjacentCell(Vector2Int cell, HashSet<Vector2Int> set, out Vector2Int neighbor)
    {
        if (set != null)
        {
            foreach (var other in set)
            {
                var dx = Mathf.Abs(cell.x - other.x);
                var dy = Mathf.Abs(cell.y - other.y);
                if ((dx > 0 || dy > 0) && dx <= 1 && dy <= 1)
                {
                    neighbor = other;
                    return true;
                }
            }
        }

        neighbor = default;
        return false;
    }

    private void UpdateRoutePulse()
    {
        if (!_highlightLayers.TryGetValue(CellState.accessibleRoutePoint, out var layer))
            return;

        if (!_baseColors.TryGetValue(CellState.accessibleRoutePoint, out var baseColor))
            baseColor = ResolveBaseColor(layer.Renderer.sharedMaterial);

        float t = 0.5f + 0.5f * Mathf.Sin(Time.time * routePulseSpeed);
        float intensity = Mathf.Lerp(1f - routePulseAmplitude, 1f + routePulseAmplitude, t);
        var color = baseColor * intensity;

        _routePulseBlock ??= new MaterialPropertyBlock();
        _routePulseBlock.SetColor("_BaseColor", color);
        _routePulseBlock.SetColor("_Color", color);
        layer.Renderer.SetPropertyBlock(_routePulseBlock);
    }

    private void UpdateFocusIndicator(Vector2Int? coords)
    {
        if (focusIndicatorPrefab == null || _coordinateMapper == null)
            return;

        if (_focusIndicator == null)
        {
            var parentTransform = focusIndicatorParent != null ? focusIndicatorParent : transform;
            _focusIndicator = Instantiate(focusIndicatorPrefab, parentTransform);
        }

        if (coords.HasValue)
        {
            var world = _coordinateMapper.ToWorld(coords.Value.x, coords.Value.y);
            _focusIndicator.transform.position = world + Vector3.up * 0.05f;
            if (!_focusIndicator.activeSelf)
            {
                _focusIndicator.SetActive(true);
            }
        }
        else if (_focusIndicator.activeSelf)
        {
            _focusIndicator.SetActive(false);
        }
    }

    private void UpdateSurfaceCollider(int width, int height)
    {
        if (!createSurfaceCollider)
        {
            if (_ownsCollider && _surfaceCollider != null)
            {
                Destroy(_surfaceCollider.gameObject);
                _surfaceCollider = null;
                _ownsCollider = false;
            }
            return;
        }

        if (_surfaceCollider == null)
        {
            var host = new GameObject("GridSurfaceCollider");
            host.layer = gameObject.layer;
            host.transform.SetParent(transform, false);
            _surfaceCollider = host.AddComponent<BoxCollider>();
            _surfaceCollider.hideFlags = HideFlags.DontSave;
            _ownsCollider = true;
        }

        var size = new Vector3(width * _cellSpacing, colliderThickness, height * _cellSpacing);
        var center = _gridOriginWorld + new Vector3(size.x * 0.5f, colliderThickness * 0.5f, size.z * 0.5f) - transform.position;

        _surfaceCollider.transform.localPosition = Vector3.zero;
        _surfaceCollider.transform.localRotation = Quaternion.identity;
        _surfaceCollider.size = size;
        _surfaceCollider.center = center;
        _surfaceCollider.enabled = true;
    }

    private void OnDestroy()
    {
        if (_ownsCollider && _surfaceCollider != null)
        {
            Destroy(_surfaceCollider.gameObject);
        }

        foreach (var layer in _highlightLayers.Values)
        {
            if (layer.GameObject != null)
            {
                Destroy(layer.GameObject);
            }
            if (layer.Mesh != null)
            {
                Destroy(layer.Mesh);
            }
        }
        _highlightLayers.Clear();
    }
}
