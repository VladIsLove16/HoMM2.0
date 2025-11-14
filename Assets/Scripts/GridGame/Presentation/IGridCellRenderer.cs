public interface IGridCellRenderer
{
    void Clear();
    void Bind(IGridViewModel viewModel);
    void Unbind(IGridViewModel viewModel);
}
