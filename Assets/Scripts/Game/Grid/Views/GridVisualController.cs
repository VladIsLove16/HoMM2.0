using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GridVisualController : MonoBehaviour
{
	[Inject] private IGridCellRenderer _renderer;

	private Vector2Int? _selected;
	private Vector2Int? _hovered;
	private List<Vector2Int> _reachable = new();
	private List<(Vector2Int, bool)> _routePoints = new();

	public void SetSelected(Vector2Int cell)
	{
		if (_selected.HasValue)
			_renderer.RemoveState(_selected.Value, CellState.selected);
		_selected = cell;
		_renderer.AddState(cell, CellState.selected);
	}

	public void SetHovered(Vector2Int cell)
	{
		if (_hovered.HasValue)
			_renderer.RemoveState(_hovered.Value, CellState.hovered);
		_hovered = cell;
		_renderer.AddState(cell, CellState.hovered);
	}

	public void ShowReachableCells(List<Vector2Int> cells)
	{
		foreach (var c in _reachable)
			_renderer.RemoveState(c, CellState.moveAvailable);
		foreach (var c in cells)
			_renderer.AddState(c, CellState.moveAvailable);
		_reachable = cells;
	}

	public void HighlightRoute(List<(Vector2Int, bool)> route)
	{
		foreach (var point in _routePoints)
			_renderer.RemoveState(point.Item1, point.Item2
				? CellState.accessibleRoutePoint
				: CellState.inaccessibleRoutePoint);

		foreach (var point in route)
			_renderer.AddState(point.Item1, point.Item2
				? CellState.accessibleRoutePoint
				: CellState.inaccessibleRoutePoint);

		_routePoints = route;
	}
}
