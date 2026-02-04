using System.Collections.Generic;
using UnityEngine;

public class PreviewResult
{
    private readonly Dictionary<CellState, List<Vector2Int>> _data = new();
    public PreviewResult()
    {
    }
    public PreviewResult(Dictionary<CellState, List<Vector2Int>> data)
    {
        _data = data;
    }

    public void Add(CellState state, IEnumerable<Vector2Int> cells)
    {
        if (!_data.ContainsKey(state))
            _data[state] = new List<Vector2Int>();
        _data[state].AddRange(cells);
    }

    public void Set(CellState state, IEnumerable<Vector2Int> cells)
    {
        _data[state] = cells == null ? new List<Vector2Int>() : new List<Vector2Int>(cells);
    }

    public void Add(Dictionary<CellState, List<Vector2Int>> dict)
    {
        foreach(var cell in dict)
        {
            Add(cell.Key,  cell.Value);
        }
    }

    public Dictionary<CellState, List<Vector2Int>> ToDictionary() => _data;
}
