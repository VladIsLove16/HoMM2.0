using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// Lightweight grid renderer that delegates to ConsoleGridState, logs grid changes,
/// and provides a simple world-to-cell conversion suitable for console scenarios.
/// </summary>
public class ConsoleGridRenderer : IGridCellRenderer, IWorldToCellProvider
{
    private readonly ConsoleGridState _gridState;
    private readonly IDeveloperConsoleOutput _output;
    private IGridViewModel _viewModel;

    public ConsoleGridRenderer(ConsoleGridState gridState, [Inject(Optional = true)] IDeveloperConsoleOutput output = null)
    {
        _gridState = gridState;
        _output = output;
    }

    public void Clear()
    {
        Log("Console grid cleared.");
    }

    public void Render(int width, int height, float cellSize, Vector3 origin, float padding)
    {
        _gridState.Initialize(width, height);
        Log($"Console grid render width={width} height={height} cellSize={cellSize} origin={origin} padding={padding}");
        Log(_gridState.BuildRepresentation());
    }

    public CellState[] GetCellStates(Vector2Int coords)
    {
        return _gridState.GetCellStates(coords);
    }

    public void Bind(IGridViewModel gameViewModel)
    {
        _viewModel = gameViewModel;
        Log("Console grid renderer bound to view model.");
    }

    public void Unbind(IGridViewModel gameViewModel)
    {
        if (_viewModel == gameViewModel)
        {
            _viewModel = null;
        }
        Log("Console grid renderer unbound.");
    }

    public Vector3 ToWorld(int x, int y)
    {
        return new Vector3(x, 0f, y);
    }

    public bool ToGrid(Vector3 position, out Vector2Int coords)
    {
        coords = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        return true;
    }

    public bool ToGridPair(Vector3 position, out KeyValuePair<Vector2Int, Vector2Int> coords)
    {
        var main = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        coords = new KeyValuePair<Vector2Int, Vector2Int>(main, main);
        return true;
    }

    private void Log(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        if (_output != null)
        {
            _output.AppendLine(message);
        }
        else
        {
            Debug.Log(message);
        }
    }
}
