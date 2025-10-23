using UniRx;

public interface ICursorViewModel
{
    public IReadOnlyReactiveProperty<CursorVisualState> CursorState {  get; }
    public IReadOnlyReactiveProperty<bool> IsLocked {  get; }
}