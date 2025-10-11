using UniRx;

public class CursorViewModel
{
    public IReadOnlyReactiveProperty<CursorVisualState> CursorState => _cursorState;
    private readonly ReactiveProperty<CursorVisualState> _cursorState = new(CursorVisualState.Hidden);

    public IReadOnlyReactiveProperty<bool> IsLocked => _isLocked;
    private readonly ReactiveProperty<bool> _isLocked = new(true);

    public void LockToCenter()
    {
        _isLocked.Value = true;
        _cursorState.Value = CursorVisualState.Hidden;
    }

    public void Unlock(CursorVisualState visual = CursorVisualState.Default)
    {
        _isLocked.Value = false;
        _cursorState.Value = visual;
    }
}