using System;
using UniRx;
using Zenject;

public class GridCursorViewModel : ICursorViewModel, IDisposable
{
    public IReadOnlyReactiveProperty<CursorVisualState> CursorState => _cursorState;
    private readonly ReactiveProperty<CursorVisualState> _cursorState = new(CursorVisualState.Default);

    public IReadOnlyReactiveProperty<bool> IsLocked => _isLocked;
    private readonly ReactiveProperty<bool> _isLocked = new(false);
    private ActionResolver _actionResolver;
    private bool _isDisposed;

    [Inject]
    void Construct(ActionResolver actionResolver)
    {
        _actionResolver = actionResolver ?? throw new ArgumentNullException(nameof(actionResolver));
        _actionResolver.ActionResolved += OnActionPreviewChanged;
        _actionResolver.ActionNotResolved += OnActionNotResolved;
    }

    private void OnActionPreviewChanged(ActionPlan plan)
    {
        var state = GetCursorStateByActionType(plan.ActionType);
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
    private void OnActionNotResolved()
    {
        if (_isLocked.Value)
            return;

        _cursorState.SetValueAndForceNotify(CursorVisualState.NotAvailable);
    }

    private CursorVisualState GetCursorStateByActionType(ActionType actionType)
    {
        switch (actionType)
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

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        if (_actionResolver != null)
        {
            _actionResolver.ActionResolved -= OnActionPreviewChanged;
            _actionResolver.ActionNotResolved -= OnActionNotResolved;
            _actionResolver = null;
        }
    }
}
