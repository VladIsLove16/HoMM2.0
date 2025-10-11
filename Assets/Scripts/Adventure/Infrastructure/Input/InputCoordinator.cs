using System;
using Zenject;

public sealed class InputCoordinator : IInitializable, IDisposable
{
    private readonly ICursorContextManager _cursorManager;
    private readonly ICursorService _cursorService;
    private readonly IInputModeService _inputModeService;

    public InputCoordinator(
        ICursorContextManager cursorManager,
        ICursorService cursorService,
        IInputModeService inputModeService)
    {
        _cursorManager = cursorManager;
        _cursorService = cursorService;
        _inputModeService = inputModeService;
    }

    public void Initialize()
    {
        _inputModeService.OnModeChanged += OnModeChanged;
    }

    public void Dispose()
    {
        _inputModeService.OnModeChanged -= OnModeChanged;
    }

    private void OnModeChanged(InputMode mode)
    {
        switch (mode)
        {
            case InputMode.Enabled:
                _cursorService.LockToCenter();
                break;
            case InputMode.Blocked:
                _cursorService.Unlock();
                break;
            case InputMode.OnlyMove:
                _cursorService.Unlock();
                break;
        }
    }
}
