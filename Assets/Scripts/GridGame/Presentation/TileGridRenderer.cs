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
    [SerializeField] private GridRenderSettingsSO editorRenderSettings;

    private Mesh _gridMesh;
    private IGridViewModel _viewModel;
    private IWorldToCellProvider _coordinateMapper;
    private GameObject _focusIndicator;
    private BoxCollider _surfaceCollider;
    private bool _ownsCollider;

    private IGridRenderSettings _renderSettings;
    private readonly Dictionary<CellState, CellMaterial> _materials = new();
    private readonly Dictionary<CellState, HighlightLayer> _highlightLayers = new();
    private readonly Dictionary<CellState, List<Vector2Int>> _cellStates = new();

    private Vector3 _gridOriginWorld;
    private float _cellSpacing = 1f;

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
    private void Construct(IGridViewModel viewModel, IWorldToCellProvider mapper)
    {
        _coordinateMapper = mapper;
        SetRenderSettings(viewModel?.RenderSettings ?? editorRenderSettings);
        Bind(viewModel);
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
        _materials.Clear();

        if (_renderSettings?.Materials != null)
        {
            foreach (var mat in _renderSettings.Materials)
            {
                if (mat == null || mat.Material == null)
                    continue;
                _materials[mat.CellState] = mat;
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

        if (!_cellStates.TryGetValue(state, out var cells) || cells == null || cells.Count == 0)
        {
            return;
        }

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
