using System;
using UniRx;
using Zenject;

public class GridCursorViewModel : ICursorViewModel
{
    public IReadOnlyReactiveProperty<CursorVisualState> CursorState => _cursorState;
    private readonly ReactiveProperty<CursorVisualState> _cursorState = new(CursorVisualState.Hidden);

    public IReadOnlyReactiveProperty<bool> IsLocked => _isLocked;
    private readonly ReactiveProperty<bool> _isLocked = new(true);
    [Inject] private ActionResolver _actionResolver;
    [Inject]
    void Construct()
    {
        _actionResolver.ActionResolved += OnActionPreviewChanged;
    }

    private void OnActionPreviewChanged((IActionHandler, ActionContext) tuple)
    {
        var state = GetCursorStateByActionHandler(tuple.Item1);
        _cursorState.SetValueAndForceNotify(state);
    }

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
    private CursorVisualState GetCursorStateByActionHandler(IActionHandler actionHandler)
    {
        switch (actionHandler.ActionType)
        {
            case ActionType.Move:
                return CursorVisualState.Move;
            case ActionType.MoveThenAttack:
            case ActionType.Attack:
                return CursorVisualState.Attack;
            case ActionType.RangedAttack:
                return CursorVisualState.RangedAttack;
            default:
                return CursorVisualState.Default;
        }
    }
}