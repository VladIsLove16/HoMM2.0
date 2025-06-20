using System;
using UnityEngine;
using Zenject;

public class GridView : MonoBehaviour
{
    [SerializeField] private Transform originPosition;
    [SerializeField] private float cellSize;
    [SerializeField] private float padding;

    private GameGridViewModel _viewModel;
    private ICellGridRenderer _gridRenderer;

    [Inject]
    public void Construct(
        GameGridViewModel viewModel,
        ICellGridRenderer renderer
    )
    {
        _viewModel = viewModel;
        _gridRenderer = renderer;
    }

    public void CreateGrid()
    {
        int width = _viewModel.GetWidth();
        int height = _viewModel.GetHeight();
        _gridRenderer.Clear();
        _gridRenderer.Render(width, height, cellSize, originPosition.position, padding);
    }

    private void UpdateCellVisual(GridCellUnitSpawnedEventArgs args)
    {
    }

    private void OnDestroy()
    {
        _gridRenderer.Clear();
    }

}
