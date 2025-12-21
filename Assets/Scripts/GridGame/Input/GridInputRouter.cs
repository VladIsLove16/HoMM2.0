using System;
using Adventure.Settings.Configuration;
using Adventure.Settings.ViewModel;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

/// <summary>
/// Routes all grid related input to the CellInputHandler and opens in-game menus.
/// Pointer handling is platform agnostic (touch, mouse, gamepad) so the handler does not care about device specifics.
/// </summary>
public sealed class GridInputRouter : ITickable, IDisposable
{
    private const float DefaultVirtualCursorSpeed = 1400f;

    private readonly CellInputHandler _cell;
    private readonly ITurnStateViewModel _turnState;
    private readonly GridGameSettingsViewModel _settingsVM;
    private readonly IMouseSensitivityService _mouseSensitivityService;

    private InputSystem_GridGame _input;
    private IDisposable _stateSubscription;

    private Vector2 _currentPointerPosition;
    private Vector2 _virtualPointerPosition;
    private Vector2 _navigationVector;
    private bool _pointerInitialized;
    private bool _useVirtualPointer;
    private bool _isDisposed;

    [Inject]
    public GridInputRouter(
        CellInputHandler cell,
        ITurnStateViewModel turnState,
        GridGameSettingsViewModel settingsVM,
        IMouseSensitivityService mouseSensitivityService = null)
    {
        _cell = cell ?? throw new ArgumentNullException(nameof(cell));
        _turnState = turnState;
        _settingsVM = settingsVM ?? throw new ArgumentNullException(nameof(settingsVM));
        _mouseSensitivityService = mouseSensitivityService;

        _input = new InputSystem_GridGame();
        WireInput();
        _input.Menus.Enable();

        if (_turnState != null)
        {
            _stateSubscription = _turnState.BattleStateProperty.Subscribe(OnTurnStateChanged);
            OnTurnStateChanged(_turnState.BattleStateProperty.Value);
        }
        else
        {
            _input.Grid.Enable();
            _input.GridPlacement.Disable();
        }
    }

    public void Tick()
    {
        UpdateVirtualPointer();

        if (_cell != null && _cell.IsDragging)
        {
            _cell.HandleDragUpdate();
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        UnwireInput();
        _stateSubscription?.Dispose();
        _stateSubscription = null;

        if (_input != null)
        {
            _input.Grid.Disable();
            _input.GridPlacement.Disable();
            _input.Menus.Disable();
            _input.Disable();
            _input.Dispose();
            _input = null;
        }
    }

    private void WireInput()
    {
        _input.Grid.MousePosition.performed += OnPointerMoved;
        _input.Grid.Select.performed += OnSelect;
        _input.Grid.Action.performed += OnAction;
        _input.Grid.Navigate.performed += OnNavigate;
        _input.Grid.Navigate.canceled += OnNavigateCanceled;

        _input.GridPlacement.Drag.started += OnDragStarted;
        _input.GridPlacement.Drag.performed += OnDragPerformed;
        _input.GridPlacement.Drag.canceled += OnDragCanceled;

        _input.Menus.OpenSettings.performed += OnOpenSettings;
    }

    private void UnwireInput()
    {
        if (_input == null)
            return;

        _input.Grid.MousePosition.performed -= OnPointerMoved;
        _input.Grid.Select.performed -= OnSelect;
        _input.Grid.Action.performed -= OnAction;
        _input.Grid.Navigate.performed -= OnNavigate;
        _input.Grid.Navigate.canceled -= OnNavigateCanceled;

        _input.GridPlacement.Drag.started -= OnDragStarted;
        _input.GridPlacement.Drag.performed -= OnDragPerformed;
        _input.GridPlacement.Drag.canceled -= OnDragCanceled;

        _input.Menus.OpenSettings.performed -= OnOpenSettings;
    }

    private void OnPointerMoved(InputAction.CallbackContext ctx)
    {
        var position = ctx.ReadValue<Vector2>();
        if (float.IsNaN(position.x) || float.IsNaN(position.y))
            return;

        _pointerInitialized = true;
        _useVirtualPointer = false;
        _currentPointerPosition = position;
        _virtualPointerPosition = position;
        _cell.UpdatePointerPosition(position);
        _cell.HandleMouseMoved();
    }

    private void OnSelect(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            _cell.HandleSelect();
        }
    }

    private void OnAction(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            _cell.HandleAction();
        }
    }

    private void OnNavigate(InputAction.CallbackContext ctx)
    {
        _navigationVector = ctx.ReadValue<Vector2>();
        if (_navigationVector.sqrMagnitude < 0.0001f)
            return;

        if (!_pointerInitialized)
        {
            _pointerInitialized = true;
            _currentPointerPosition = GetScreenCenter();
            _virtualPointerPosition = _currentPointerPosition;
            _cell.UpdatePointerPosition(_currentPointerPosition);
            _cell.HandleMouseMoved();
        }

        _useVirtualPointer = true;
    }

    private void OnNavigateCanceled(InputAction.CallbackContext ctx)
    {
        _navigationVector = Vector2.zero;
    }

    private void OnDragStarted(InputAction.CallbackContext ctx)
    {
        _cell.HandleDragStart();
    }

    private void OnDragPerformed(InputAction.CallbackContext ctx)
    {
        _cell.HandleDragUpdate();
    }

    private void OnDragCanceled(InputAction.CallbackContext ctx)
    {
        _cell.HandleDragCancel();
    }

    private void OnOpenSettings(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            _settingsVM.Toggle();
        }
    }

    private void UpdateVirtualPointer()
    {
        if (!_useVirtualPointer || _navigationVector.sqrMagnitude < 0.0001f)
            return;

        var speed = _mouseSensitivityService?.CursorSpeed ?? DefaultVirtualCursorSpeed;
        var delta = _navigationVector * speed * Time.unscaledDeltaTime;
        _virtualPointerPosition = ClampToScreen(_virtualPointerPosition + delta);
        _currentPointerPosition = _virtualPointerPosition;
        _cell.UpdatePointerPosition(_virtualPointerPosition);
        _cell.HandleMouseMoved();
    }

    private static Vector2 ClampToScreen(Vector2 position)
    {
        var width = Mathf.Max(1, Screen.width);
        var height = Mathf.Max(1, Screen.height);
        return new Vector2(
            Mathf.Clamp(position.x, 0f, width),
            Mathf.Clamp(position.y, 0f, height));
    }

    private static Vector2 GetScreenCenter()
    {
        return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    private void OnTurnStateChanged(BattleState state)
    {
        if (_input == null)
            return;

        switch (state)
        {
            case BattleState.replacement:
                _input.Grid.Disable();
                _input.GridPlacement.Enable();
                break;
            case BattleState.inProgress:
                _input.Grid.Enable();
                _input.GridPlacement.Disable();
                break;
        }
    }
}
