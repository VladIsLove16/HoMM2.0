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
    private IGridCellRenderer _cellGridRenderer;
    private CellState previousCellState;
    private Vector2Int previousHoveredCell;
    [Inject]
    public void Construct(
        GameViewModel viewModel,
        IGridCellRenderer renderer
    )
    {
        Debug.Log("GridView construct");
        _viewModel = viewModel;
        _cellGridRenderer = renderer;
    }



    public void CreateGrid()
    {
        int width = _viewModel.GetWidth();
        int height = _viewModel.GetHeight();
        _cellGridRenderer.Clear();
        _cellGridRenderer.Render(width, height, cellSize, originPosition.position, padding);
    }
    private void OnDestroy()
    {
        _cellGridRenderer.Clear();
    }

}
