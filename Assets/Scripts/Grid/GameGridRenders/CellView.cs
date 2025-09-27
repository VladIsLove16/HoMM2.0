using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class CellView : MonoBehaviour, IGameViewObject
{
    private Renderer _renderer;
    [SerializeField] private MeshRenderer _hoverRenderer;
    [SerializeField] private MeshRenderer _routePointRenderer;
    private IReadOnlyDictionary<CellState, CellMaterial> _materials;
    [SerializeField] private List<CellState> cellStates = new();

    public bool IsHoverable => true;
    public bool IsSelectable => true;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }
    private void Start()
    {
        UpdateView();
    }
    internal void Init(IReadOnlyDictionary<CellState, CellMaterial> materials)
    {
        _materials = materials;
    }

    // Public helpers for tests/runtime wiring
    public void SetHoverRenderer(MeshRenderer r)
    {
        _hoverRenderer = r;
    }

    public void SetRoutePointRenderer(MeshRenderer r)
    {
        _routePointRenderer = r;
    }

    public void SetMaterialsDictionary(IReadOnlyDictionary<CellState, CellMaterial> materials)
    {
        _materials = materials;
        UpdateView();
    }

    public void AddState(CellState state)
    {
        if(!cellStates.Contains(state))
        {
            cellStates.Add(state);
            UpdateView();
        }
    }


    public void RemoveState(CellState state)
    {
        cellStates.Remove(state);
        UpdateView();
    }

    public CellState[] GetStates()
    {
        return cellStates.ToArray();
    }

    private void UpdateView()
    {
        if (cellStates.Contains(CellState.hovered))
        {
            _hoverRenderer.gameObject.SetActive(true);
        }
        else
            _hoverRenderer.gameObject.SetActive(false);

        if (cellStates.Contains(CellState.accessibleRoutePoint) || cellStates.Contains(CellState.inaccessibleRoutePoint))
            _routePointRenderer.gameObject.SetActive(true);
        else
            _routePointRenderer.gameObject.SetActive(false);

        if (cellStates.Contains(CellState.accessibleRoutePoint))
            _routePointRenderer.material = _materials[CellState.accessibleRoutePoint].Material;

        if (cellStates.Contains(CellState.inaccessibleRoutePoint))
            _routePointRenderer.material = _materials[CellState.inaccessibleRoutePoint].Material;


        // Safely set materials only if materials dictionary is available and contains keys
        if (_materials != null)
        {
            if (cellStates.Contains(CellState.selected) && _materials.ContainsKey(CellState.selected))
            {
                SetMaterial(_materials[CellState.selected].Material);
                return;
            }
            if (cellStates.Contains(CellState.reachableCell) && _materials.ContainsKey(CellState.reachableCell))
            {
                SetMaterial(_materials[CellState.reachableCell].Material);
                return;
            }
            if (_materials.ContainsKey(CellState.normal))
            {
                SetMaterial(_materials[CellState.normal].Material);
                return;
            }
        }
        // fallback: do nothing if materials are not ready
    }
    private void SetMaterial(Material material)
    {
        _renderer.material = material;
    }

}