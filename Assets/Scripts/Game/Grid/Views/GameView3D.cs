using UnityEngine;
using Zenject;

public class GameView3D : MonoBehaviour
{
    [Inject] private GameViewModel _viewModel;
    [Inject] private GameInputHandler3D _input;
    [Inject] private UnitViewManager3D _unitManager;
    [Inject] private GridVisualController _gridVisual;

    private void Start()
    {
        _viewModel.CellContentAdded += _unitManager.Add;
        _viewModel.CellContentRemoved += _unitManager.Remove;
        _viewModel.CellContentMovedByRoute += _unitManager.MoveAlongRoute;
        _viewModel.CellContentSwaped += _unitManager.Swap;

        _viewModel.ReachableCellsChanged += _gridVisual.ShowReachableCells;
        _viewModel.RoutePointsChanged += _gridVisual.HighlightRoute;
        _viewModel.CellSelected += _gridVisual.SetSelected;

        _input.CellHovered += _viewModel.HandleCellHovered;
        _input.CellSelected += _viewModel.HandleCellSelected;
        _input.ActionRequested += () => _viewModel.PerformAction(_input.CurrentHoveredCell);
    }
}
