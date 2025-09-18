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
    private IReadOnlyDictionary<CellState, CellMaterials> _materials;
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
    internal void Init(IReadOnlyDictionary<CellState, CellMaterials> materials)
    {
        _materials = materials;
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


        if (cellStates.Contains(CellState.selected))
        {
            SetMaterial(_materials[CellState.selected].Material);
        }
        else if (cellStates.Contains(CellState.moveAvailable))
        {
            SetMaterial(_materials[CellState.moveAvailable].Material);
        }
        else
            SetMaterial(_materials[CellState.normal].Material);
    }
    private void SetMaterial(Material material)
    {
        _renderer.material = material;
    }

}