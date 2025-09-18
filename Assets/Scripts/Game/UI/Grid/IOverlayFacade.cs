using System.Collections.Generic;
using UnityEngine;

public interface IOverlayFacade
{
    void SetStates(IEnumerable<Vector2Int> cells, CellState state);
    void RemoveByState(CellState state);
    void ClearPreview();
}


