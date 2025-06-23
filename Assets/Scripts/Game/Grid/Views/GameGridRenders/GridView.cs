using System;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class GridView : MonoBehaviour
{
    [SerializeField] private Transform originPosition;
    [SerializeField] private float cellSize;
    [SerializeField] private float padding;

    private GameViewModel _viewModel;
    private ICellGridRenderer _cellGridRenderer;

    private Vector2Int _selectedGameCell;
    private bool isCellSelected;
    private CellState previousCellState;
    private Vector2Int previousHoveredCell;
    [Inject]
    public void Construct(
        GameViewModel viewModel,
        ICellGridRenderer renderer
    )
    {
        Debug.Log("GridView construct");
        _viewModel = viewModel;
        _cellGridRenderer = renderer;
    }

    private void Update()
    {
        if (_viewModel.IsPlayerTurn())
        {
            HoverCell();
        }
        else if (isCellSelected)
            UnSelect();

            
    }

    private void HoverCell()
    {
        Mouse3D.GetMouseWorldPosition(out Vector3 mpusePos);
        _cellGridRenderer.ToGrid(mpusePos, out Vector2Int cellCoords);
        _cellGridRenderer.SetCellState(cellCoords, CellState.hovered);
    }

    public void CreateGrid()
    {
        int width = _viewModel.GetWidth();
        int height = _viewModel.GetHeight();
        _cellGridRenderer.Clear();
        _cellGridRenderer.Render(width, height, cellSize, originPosition.position, padding);
    }

    public void SelectCell(Vector2Int cellCoords)
    {
        UnSelect();
        _cellGridRenderer.SetCellState(cellCoords, CellState.selected);
        _selectedGameCell = cellCoords;
        isCellSelected = true;
    }

    public void UnSelect()
    {
        if (isCellSelected)
        {
            _cellGridRenderer.SetCellState(_selectedGameCell, CellState.normal);
            isCellSelected = false;
        }
    }
    public bool GetCell(Vector3 position, out Vector2Int coords)
    {
        return _cellGridRenderer.ToGrid(position, out coords);
    }

    private void OnDestroy()
    {
        _cellGridRenderer.Clear();
    }

    internal void Hover(Vector2Int coords)
    {
        _cellGridRenderer.SetCellState(previousHoveredCell, previousCellState);
        previousHoveredCell = coords;
        previousCellState = _cellGridRenderer.GetCellState(coords);
        _cellGridRenderer.SetCellState(coords, CellState.hovered);
    }

}
