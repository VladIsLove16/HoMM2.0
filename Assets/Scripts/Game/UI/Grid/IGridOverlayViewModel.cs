using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public interface IGridOverlayViewModel
{
    IReadOnlyDictionary<Vector2Int, CellState> States { get; }
    ReactiveProperty<Vector2Int?> Hovered { get; }
    ReactiveProperty<Vector2Int?> Selected { get; }
    IObservable<UniRx.Unit> Changed { get; }

    void SetStates(IEnumerable<Vector2Int> cells, CellState state);
    void RemoveStates(CellState state);
    void Clear();
}


