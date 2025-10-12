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
        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
        }
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
        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer == null)
            {
                return;
            }
        }

        if (_hoverRenderer != null)
        {
            if (cellStates.Contains(CellState.hovered))
            {
                _hoverRenderer.gameObject.SetActive(true);
            }
            else
            {
                _hoverRenderer.gameObject.SetActive(false);
            }
        }

        if (_routePointRenderer != null)
        {
            if (cellStates.Contains(CellState.accessibleRoutePoint) || cellStates.Contains(CellState.inaccessibleRoutePoint))
                _routePointRenderer.gameObject.SetActive(true);
            else
                _routePointRenderer.gameObject.SetActive(false);

            if (cellStates.Contains(CellState.accessibleRoutePoint) && _materials != null && _materials.ContainsKey(CellState.accessibleRoutePoint))
                _routePointRenderer.material = _materials[CellState.accessibleRoutePoint].Material;

            if (cellStates.Contains(CellState.inaccessibleRoutePoint) && _materials != null && _materials.ContainsKey(CellState.inaccessibleRoutePoint))
                _routePointRenderer.material = _materials[CellState.inaccessibleRoutePoint].Material;
        }

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
            if (cellStates.Contains(CellState.enemyReachableCell) && _materials.ContainsKey(CellState.enemyReachableCell))
            {
                SetMaterial(_materials[CellState.enemyReachableCell].Material);
                return;
            }
            if (_materials.ContainsKey(CellState.normal))
            {
                SetMaterial(_materials[CellState.normal].Material);
                return;
            }
        }

       
    }
    private void SetMaterial(Material material)
    {
        if (material == null)
        {
            return;
        }

        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer == null)
            {
                return;
            }
        }

        _renderer.material = material;
    }

}
