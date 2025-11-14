using System.Collections.Generic;
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

    private Mesh _gridMesh;
    private IGridViewModel _viewModel;
    private IWorldToCellProvider _coordinateMapper;
    private GameObject _focusIndicator;
    private BoxCollider _surfaceCollider;
    private bool _ownsCollider;

    [Inject]
    private void Construct(IGridViewModel viewModel, IWorldToCellProvider mapper)
    {
        _coordinateMapper = mapper;
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

        _viewModel.GridInited += OnGridInited;
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

        var settings = _viewModel?.RenderSettings;
        var spacing = (settings?.CellSize ?? 1f) + (settings?.CellPadding ?? 0f);
        var origin = transform.position - new Vector3(spacing * 0.5f, 0f, spacing * 0.5f);

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
            var start = origin + new Vector3(x * spacing, 0f, 0f);
            var end = start + new Vector3(0f, 0f, height * spacing);
            lines.Add(start);
            lines.Add(end);
        }

        for (int y = 0; y <= height; y++)
        {
            var start = origin + new Vector3(0f, 0f, y * spacing);
            var end = start + new Vector3(width * spacing, 0f, 0f);
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
        UpdateSurfaceCollider(width, height, spacing, origin);
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

    private void OnPreviewUpdated(PreviewResult result)
    {
        if (result == null)
        {
            UpdateFocusIndicator(null);
            return;
        }

        var dict = result.ToDictionary();
        Vector2Int? coords = null;
        if (dict != null && dict.TryGetValue(CellState.hovered, out var cells) && cells != null && cells.Count > 0)
        {
            coords = cells[0];
        }

        UpdateFocusIndicator(coords);
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

    private void UpdateSurfaceCollider(int width, int height, float spacing, Vector3 origin)
    {
        if (!createSurfaceCollider)
            return;

        if (_surfaceCollider == null)
        {
            var host = new GameObject("GridSurfaceCollider");
            host.layer = gameObject.layer;
            host.transform.SetParent(transform, false);
            _surfaceCollider = host.AddComponent<BoxCollider>();
            _surfaceCollider.hideFlags = HideFlags.DontSave;
            _ownsCollider = true;
        }

        var size = new Vector3(width * spacing, colliderThickness, height * spacing);
        var center = origin + new Vector3(size.x * 0.5f, colliderThickness * 0.5f, size.z * 0.5f) - transform.position;

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
    }
}
