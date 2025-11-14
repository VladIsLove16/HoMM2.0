using UnityEngine;

/// <summary>
/// Console-friendly implementation of <see cref="IGridCellRenderer"/> that keeps the
/// <see cref="ConsoleGridState"/> in sync with the current <see cref="IGridViewModel"/>.
/// Instead of spawning GameObjects it logs updates and relies on textual output.
/// </summary>
public class ConsoleGridRenderer : IGridCellRenderer
{
    private readonly ConsoleGridState _gridState;
    private IGridViewModel _viewModel;
    private int _width;
    private int _height;

    public ConsoleGridRenderer(ConsoleGridState gridState)
    {
        _gridState = gridState;
    }

    public void Bind(IGridViewModel viewModel)
    {
        if (_viewModel == viewModel)
            return;

        if (_viewModel != null)
        {
            Unbind(_viewModel);
        }

        _viewModel = viewModel;
        if (_viewModel == null)
            return;

        _viewModel.GridInited += OnGridInited;
    }

    public void Unbind(IGridViewModel viewModel)
    {
        if (_viewModel != viewModel || _viewModel == null)
            return;

        _viewModel.GridInited -= OnGridInited;
        _viewModel = null;
    }

    public void Clear()
    {
        if (_width > 0 && _height > 0)
        {
            _gridState.Initialize(_width, _height);
            Log("Console grid cleared.");
        }
    }

    private void OnGridInited(int width, int height)
    {
        _width = width;
        _height = height;
        _gridState.Initialize(width, height);
        Log($"Console grid initialized {width}x{height}");
    }

    private void Log(string message)
    {
        Debug.Log(message);
    }
}
