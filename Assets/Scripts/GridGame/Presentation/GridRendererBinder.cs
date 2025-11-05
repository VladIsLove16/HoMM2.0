using Zenject;

/// <summary>
/// Ensures the active grid renderer is bound to the grid view model after all bindings are installed.
/// </summary>
public sealed class GridRendererBinder : IInitializable
{
    private readonly IGridCellRenderer _renderer;
    private readonly IGridViewModel _viewModel;

    public GridRendererBinder(IGridCellRenderer renderer, IGridViewModel viewModel)
    {
        _renderer = renderer;
        _viewModel = viewModel;
    }

    public void Initialize()
    {
        _renderer.Bind(_viewModel);
    }
}
