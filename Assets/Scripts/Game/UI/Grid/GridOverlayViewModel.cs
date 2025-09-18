using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Pool;

public class GridOverlayViewModel : IGridOverlayViewModel
{
    private readonly Dictionary<Vector2Int, CellState> states = new Dictionary<Vector2Int, CellState>();
    private readonly Subject<UniRx.Unit> changed = new Subject<UniRx.Unit>();

    public IReadOnlyDictionary<Vector2Int, CellState> States => states;
    public ReactiveProperty<Vector2Int?> Hovered { get; } = new ReactiveProperty<Vector2Int?>();
    public ReactiveProperty<Vector2Int?> Selected { get; } = new ReactiveProperty<Vector2Int?>();
    public IObservable<UniRx.Unit> Changed => changed;

    public void SetStates(IEnumerable<Vector2Int> cells, CellState state)
    {
        foreach (var c in cells)
        {
            states[c] = state;
        }
        changed.OnNext(UniRx.Unit.Default);
    }

    public void RemoveStates(CellState state)
    {
        var toRemove = ListPool<Vector2Int>.Get();
        foreach (var kv in states)
        {
            if (kv.Value == state) toRemove.Add(kv.Key);
        }
        foreach (var key in toRemove) states.Remove(key);
        ListPool<Vector2Int>.Release(toRemove);
        changed.OnNext(UniRx.Unit.Default);
    }

    public void Clear()
    {
        states.Clear();
        changed.OnNext(UniRx.Unit.Default);
    }
}


