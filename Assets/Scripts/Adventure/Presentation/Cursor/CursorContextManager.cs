using System.Collections.Generic;
using System.Linq;
using Zenject;

public sealed class CursorContextManager : ICursorContextManager
{
    private readonly ICursorService _cursorService;
    private readonly Stack<ICursorContext> _activeContexts = new Stack<ICursorContext>();

    [Inject]
    public CursorContextManager(ICursorService cursorService)
    {
        _cursorService = cursorService;
    }

    public void RequestCursor(ICursorContext context)
    {
        if (context == null)
            return;

        // если новый контекст эксклюзивный — очищаем всё остальное
        if (context.IsExclusive)
            _activeContexts.Clear();

        _activeContexts.Push(context);
        ApplyTopContext();
    }

    public void ReleaseCursor(ICursorContext context)
    {
        if (_activeContexts.Contains(context))
        {
            // удаляем конкретный контекст
            var remaining = _activeContexts.Where(c => c != context).ToList();
            _activeContexts.Clear();
            foreach (var c in remaining)
                _activeContexts.Push(c);

            ApplyTopContext();
        }
    }

    public void ForceReset()
    {
        _activeContexts.Clear();
        _cursorService.LockToCenter();
    }

    private void ApplyTopContext()
    {
        if (_activeContexts.Count == 0)
        {
            _cursorService.LockToCenter();
            return;
        }

        var top = _activeContexts.Peek();

        if (top.DesiredCursorState == CursorVisualState.Default)
            _cursorService.Unlock();
        else
        {
            _cursorService.SetCursorState(top.DesiredCursorState);
            if (top.IsExclusive)
                _cursorService.Unlock();
            else
                _cursorService.LockToCenter();
        }
    }
}
