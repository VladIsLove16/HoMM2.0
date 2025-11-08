#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using UnityEngine.InputSystem;
using Zenject;

/// <summary>
/// Routes Input System events to the developer console service/view.
/// Separated into its own class so the runtime game assemblies do not depend on console code.
/// </summary>
public sealed class DeveloperConsoleInput : IInitializable, IDisposable
{
    private readonly DeveloperConsoleService _service;
    private readonly DeveloperConsoleView _view;
    private readonly InputSystem_Console _input = new();

    public DeveloperConsoleInput(
        DeveloperConsoleService service,
        [Inject(Optional = true)] DeveloperConsoleView view = null)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _view = view;
    }

    public void Initialize()
    {
        _input.Console.Toggle.performed += OnToggle;
        _input.Console.Submit.performed += OnSubmit;
        _input.Console.Close.performed += OnClose;
        _input.Console.HistoryUp.performed += OnHistoryUp;
        _input.Console.HistoryDown.performed += OnHistoryDown;
        _input.Console.Enable();
    }

    public void Dispose()
    {
        _input.Console.Toggle.performed -= OnToggle;
        _input.Console.Submit.performed -= OnSubmit;
        _input.Console.Close.performed -= OnClose;
        _input.Console.HistoryUp.performed -= OnHistoryUp;
        _input.Console.HistoryDown.performed -= OnHistoryDown;
        _input.Dispose();
    }

    private void OnToggle(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        if (_view != null)
        {
            _view.ToggleVisibility();
        }
        else
        {
            _service.ToggleVisibility();
        }
    }

    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        _view?.SubmitFromInputActions();
    }

    private void OnClose(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        if (_view != null)
        {
            _view.Close();
        }
        else
        {
            _service.SetVisibility(false);
        }
    }

    private void OnHistoryUp(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            _view?.HistoryUp();
    }

    private void OnHistoryDown(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            _view?.HistoryDown();
    }
}

#endif
