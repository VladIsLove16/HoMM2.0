using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Maintains a textual representation of the battle grid for the console view.
/// Tracks cell previews and unit positions independently from the 3D scene.
/// </summary>
public class ConsoleGridState
{
    private readonly Dictionary<Vector2Int, ConsoleCellInfo> _cells = new();
    private readonly Dictionary<UnitViewModel, ConsoleUnitSnapshot> _unitSnapshots = new();

    private int _width;
    private int _height;

    private static readonly CellState[] _statePriority =
    {
        CellState.selected,
        CellState.attackTarget,
        CellState.hovered,
        CellState.hoveredMyTurn,
        CellState.hoveredNotMyTurn,
        CellState.reachableCell,
        CellState.enemyReachableCell,
        CellState.accessibleRoutePoint,
        CellState.inaccessibleRoutePoint
    };

    private static readonly Dictionary<CellState, char> _stateMarkers = new()
    {
        { CellState.selected, 'S' },
        { CellState.attackTarget, '!' },
        { CellState.hovered, 'H' },
        { CellState.hoveredMyTurn, 'M' },
        { CellState.hoveredNotMyTurn, 'N' },
        { CellState.reachableCell, 'R' },
        { CellState.enemyReachableCell, 'E' },
        { CellState.accessibleRoutePoint, 'a' },
        { CellState.inaccessibleRoutePoint, 'x' }
    };

    public void Initialize(int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

        _width = width;
        _height = height;

        _cells.Clear();
        _unitSnapshots.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _cells[new Vector2Int(x, y)] = new ConsoleCellInfo();
            }
        }
    }

    public void UpsertUnit(UnitViewModel viewModel)
    {
        if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
        if (!_cells.Any()) return;

        var position = viewModel.Model.Position.Value;
        if (!_cells.ContainsKey(position))
        {
            return;
        }

        if (!_unitSnapshots.TryGetValue(viewModel, out var snapshot))
        {
            snapshot = new ConsoleUnitSnapshot(viewModel);
            _unitSnapshots.Add(viewModel, snapshot);
        }

        DetachSnapshot(snapshot);
        snapshot.UpdateFromModel();

        _cells[position].Unit = snapshot;
    }

    public void MoveUnit(UnitViewModel viewModel, Vector2Int newPosition)
    {
        if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
        if (!_cells.ContainsKey(newPosition))
        {
            return;
        }

        if (!_unitSnapshots.TryGetValue(viewModel, out var snapshot))
        {
            snapshot = new ConsoleUnitSnapshot(viewModel);
            _unitSnapshots.Add(viewModel, snapshot);
        }

        DetachSnapshot(snapshot);
        snapshot.UpdateFromModel(newPosition);
        _cells[newPosition].Unit = snapshot;
    }

    public void RemoveUnit(UnitViewModel viewModel)
    {
        if (viewModel == null)
        {
            return;
        }

        if (_unitSnapshots.TryGetValue(viewModel, out var snapshot))
        {
            DetachSnapshot(snapshot);
            _unitSnapshots.Remove(viewModel);
        }
    }

    public CellState[] GetCellStates(Vector2Int coords)
    {
        if (_cells.TryGetValue(coords, out var cell))
        {
            return cell.States.ToArray();
        }
        return Array.Empty<CellState>();
    }

    private void DetachSnapshot(ConsoleUnitSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return;
        }

        if (_cells.TryGetValue(snapshot.Position, out var cell) && cell.Unit == snapshot)
        {
            cell.Unit = null;
        }
    }

    public void UpdatePreview(Dictionary<CellState, List<Vector2Int>> data, bool replace)
    {
        if (replace)
        {
            ClearAllStates();
        }

        if (data == null)
        {
            return;
        }

        foreach (var kv in data)
        {
            RemoveState(kv.Key);
            foreach (var coord in kv.Value)
            {
                if (_cells.TryGetValue(coord, out var cell))
                {
                    cell.States.Add(kv.Key);
                }
            }
        }
    }

    public string BuildRepresentation()
    {
        if (_width == 0 || _height == 0)
        {
            return "Console grid not initialized.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Console Grid {_width}x{_height}");

        for (int y = _height - 1; y >= 0; y--)
        {
            sb.Append(y.ToString("D2")).Append(" | ");
            for (int x = 0; x < _width; x++)
            {
                var coord = new Vector2Int(x, y);
                var cell = _cells[coord];
                char occupant = cell.Unit?.Symbol ?? '.';
                char stateMarker = ResolveStateMarker(cell.States);
                sb.Append(occupant);
                sb.Append(stateMarker);
                sb.Append(' ');
            }
            sb.AppendLine();
        }

        sb.Append("    ");
        for (int x = 0; x < _width; x++)
        {
            sb.Append(x.ToString("D2")).Append(' ');
        }
        sb.AppendLine();

        if (_unitSnapshots.Count > 0)
        {
            sb.AppendLine("Units:");
            foreach (var snapshot in _unitSnapshots.Values.OrderBy(u => u.Position.y).ThenBy(u => u.Position.x))
            {
                sb.Append(" - ")
                  .Append(snapshot.UnitType)
                  .Append(" [")
                  .Append(snapshot.Team)
                  .Append("] at (")
                  .Append(snapshot.Position.x)
                  .Append(',')
                  .Append(snapshot.Position.y)
                  .Append(") amt=")
                  .Append(snapshot.Amount)
                  .Append(" hp=")
                  .Append(snapshot.Health)
                  .Append('/')
                  .Append(snapshot.MaxHealth)
                  .AppendLine();
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static char ResolveStateMarker(HashSet<CellState> states)
    {
        if (states == null || states.Count == 0)
        {
            return ' ';
        }

        foreach (var state in _statePriority)
        {
            if (states.Contains(state) && _stateMarkers.TryGetValue(state, out var marker))
            {
                return marker;
            }
        }

        return ' ';
    }

    private void ClearAllStates()
    {
        foreach (var cell in _cells.Values)
        {
            cell.States.Clear();
        }
    }

    private void RemoveState(CellState state)
    {
        foreach (var cell in _cells.Values)
        {
            cell.States.Remove(state);
        }
    }

    private class ConsoleCellInfo
    {
        public ConsoleUnitSnapshot Unit;
        public readonly HashSet<CellState> States = new();
    }

    private class ConsoleUnitSnapshot
    {
        private readonly UnitViewModel _viewModel;

        public Vector2Int Position { get; private set; }
        public UnitType UnitType { get; private set; }
        public Team Team { get; private set; }
        public int Amount { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }

        public char Symbol => GetSymbol();

        public ConsoleUnitSnapshot(UnitViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            UpdateFromModel();
        }

        public void UpdateFromModel()
        {
            UpdateFromModel(_viewModel.Model.Position.Value);
        }

        public void UpdateFromModel(Vector2Int position)
        {
            Position = position;
            var model = _viewModel.Model;
            UnitType = model.UnitType.Value;
            Team = model.Team.Value;
            Amount = model.Amount.Value;
            Health = model.ModifiedStats.Health;
            MaxHealth = model.ModifiedStats.MaxHealth;
        }

        private char GetSymbol()
        {
            var raw = UnitType.ToString();
            if (string.IsNullOrEmpty(raw))
            {
                return '?';
            }
            char baseChar = char.ToUpper(raw[0]);
            return Team == Team.Blue ? baseChar : char.ToLower(baseChar);
        }
    }
}
