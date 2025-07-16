using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CellView : MonoBehaviour
{
    private Renderer _renderer;
    [SerializeField] private MeshRenderer _hoverRenderer;
    [SerializeField] private MeshRenderer _routePointRenderer;
    private IReadOnlyDictionary<CellState, CellMaterials> materials;
    [SerializeField] private List<CellState> cellStates = new();

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        UpdateView();
    }

    public void Init(IReadOnlyDictionary<CellState, CellMaterials> materials)
    {
        this.materials = materials;
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
            _routePointRenderer.material = materials[CellState.accessibleRoutePoint].Material;

        if (cellStates.Contains(CellState.inaccessibleRoutePoint))
            _routePointRenderer.material = materials[CellState.inaccessibleRoutePoint].Material;


        if (cellStates.Contains(CellState.selected))
        {
            SetMaterial(materials[CellState.selected].Material);
        }
        else if (cellStates.Contains(CellState.moveAvailable))
        {
            SetMaterial(materials[CellState.moveAvailable].Material);
        }
        else
            SetMaterial(materials[CellState.normal].Material);
    }
    private void SetMaterial(Material material)
    {
        _renderer.material = material;
    }
}