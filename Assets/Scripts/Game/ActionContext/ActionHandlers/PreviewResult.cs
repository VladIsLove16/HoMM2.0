using System.Collections.Generic;
using UnityEngine;

public class PreviewResult
{
    private readonly Dictionary<CellState, List<Vector2Int>> _data = new();

    public void Add(CellState state, IEnumerable<Vector2Int> cells)
    {
        if (!_data.ContainsKey(state))
            _data[state] = new List<Vector2Int>();
        _data[state].AddRange(cells);
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