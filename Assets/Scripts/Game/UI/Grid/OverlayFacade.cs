using System.Collections.Generic;
using UnityEngine;

public class OverlayFacade : IOverlayFacade
{
    private readonly IGridOverlayViewModel vm;

    public OverlayFacade(IGridOverlayViewModel vm)
    {
        this.vm = vm;
    }

    public void SetStates(IEnumerable<Vector2Int> cells, CellState state)
    {
        vm.SetStates(cells, state);
    }

    public void RemoveByState(CellState state)
    {
        vm.RemoveStates(state);
    }

    public void ClearPreview()
    {
        vm.Clear();
    }
}


