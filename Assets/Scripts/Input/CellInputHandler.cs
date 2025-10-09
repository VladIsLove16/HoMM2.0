using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

// Publishes high-level input signals (hover, select, action) in grid coordinates.
// Knows nothing about turns, rendering, or game rules.
public class CellInputHandler : MonoBehaviour
{
    [SerializeField] private LayerMask mouseColliderLayerMask;
    public enum SelectionMode { GridOnly, UnitThenGrid }
    [SerializeField] private SelectionMode selectionMode = SelectionMode.UnitThenGrid;

    private InputSystem_Actions inputActions;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameView3D _gameView3D;
    private ITurnStateViewModel _turnState;
    private IDisposable _battleStateSubscription;
    private readonly System.Collections.Generic.List<RaycastResult> uiRaycastResults = new System.Collections.Generic.List<RaycastResult>(8);
    private Collider _lastHoveredCollider;
    public Action ActionCanceled;
    public bool isDragging;
    public enum InputInteractionType { Hover, Select, Action, BeginDrag, DragUpdate, EndDrag }
    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        EnsureInputActionsInitialized();
    }
    private void EnsureInputActionsInitialized()
    {
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
        }
    }

    [Inject]
    public void Construct(ITurnStateViewModel turnState)
    {
        _turnState = turnState;
        EnsureInputActionsInitialized();
        _battleStateSubscription = _turnState.BattleStateProperty.Subscribe(OnTurnStateChanged);
        OnTurnStateChanged(_turnState.BattleStateProperty.Value);
    }
    private void OnEnable()
    {
        EnsureInputActionsInitialized();

        inputActions.Grid.MousePosition.performed += OnMouseMoved;
        inputActions.Grid.Select.performed += OnSelectPerformed;
        inputActions.Grid.Action.performed += OnActionPerformed;

        inputActions.GridPlacement.Drag.started += OnDragStarted;
        inputActions.GridPlacement.Drag.performed += OnDragPerformed;
        inputActions.GridPlacement.Drag.canceled += OnDragCanceled;

        if (_turnState != null)
        {
            OnTurnStateChanged(_turnState.BattleStateProperty.Value);
        }
        else
        {
            inputActions.Grid.Enable();
            inputActions.GridPlacement.Disable();
        }
    }

    private void OnTurnStateChanged(BattleState state)
    {
        EnsureInputActionsInitialized();

        switch (state)
        {
            case BattleState.replacement:
                inputActions.Grid.Disable();
                inputActions.GridPlacement.Enable();
                break;
            case BattleState.inProgress:
                inputActions.Grid.Enable();
                inputActions.GridPlacement.Disable();
                break;
        }
    }

    private void OnDisable()
    {
        if (inputActions == null)
        {
            return;
        }

        inputActions.Grid.MousePosition.performed -= OnMouseMoved;
        inputActions.Grid.Select.performed -= OnSelectPerformed;
        inputActions.Grid.Action.performed -= OnActionPerformed;
        inputActions.GridPlacement.Drag.started -= OnDragStarted;
        inputActions.GridPlacement.Drag.performed -= OnDragPerformed;
        inputActions.GridPlacement.Drag.canceled -= OnDragCanceled;
        inputActions.Disable();
        isDragging = false;
    }

    private void OnDestroy()
    {
        _battleStateSubscription?.Dispose();
        _battleStateSubscription = null;
    }

    private void Update()
    {
        if(isDragging)
        {
            Debug.Log("update DRAG");
            var worldPos = GetMouseWorldPosition();
            _gameView3D.UpdateDrag(worldPos);
        }
    }
    private void OnMouseMoved(InputAction.CallbackContext ctx)
    {
        if (TryGetHit(out var hit))
        {
            if (_lastHoveredCollider == hit.collider)
            {
                return;
            }
            var gameViewObject = hit.collider.GetComponent<IGameViewObject>();
            if (gameViewObject != null && gameViewObject.IsHoverable)
            {
                Debug.Log(gameViewObject.transform.gameObject.name + " hovered");
                _gameView3D?.HandleGameViewObjectHovered(gameViewObject);
            }
            else
                Debug.Log(hit.collider.gameObject.name + " is not IGameViewObject");
            _lastHoveredCollider = hit.collider;
        }
    }

    private void OnSelectPerformed(InputAction.CallbackContext obj)
    {
        if (IsPointerOverUI())
            return;
        if (!TryGetHit(out var hit))
        {
            Debug.Log("no hit");
            return;
        }
        var gameViewObject = hit.collider.GetComponent<IGameViewObject>();
        if (gameViewObject == null || !gameViewObject.IsSelectable)
        {
            Debug.Log(hit.collider.name + "is not selectable");
            return;
        }
        Debug.Log(hit.collider.name + " selected");
        ProcessGameViewObject(gameViewObject, InputInteractionType.Select);
    }

    private void OnActionPerformed(InputAction.CallbackContext obj)
    {
        if (IsPointerOverUI())
            return;
        if (!TryGetHit(out var hit))
        {
            ActionCanceled?.Invoke();
            return;
        }
        var gameViewObject = hit.collider.GetComponent<IGameViewObject>();
        ProcessGameViewObject(gameViewObject, InputInteractionType.Action);
    }

    private void OnDragStarted(InputAction.CallbackContext obj)
    {
        if (IsPointerOverUI())
            return;

        if (!TryGetHit(out var hit))
        {
            return;
        }
        var gameViewObject = hit.collider.GetComponent<IGameViewObject>();
        ProcessGameViewObject(gameViewObject, InputInteractionType.BeginDrag);
    }

    private void OnDragPerformed(InputAction.CallbackContext obj)
    {
        if (!isDragging)
            return;

        if (!TryGetHit(out var hit))
        {
            ProcessGameViewObject(null, InputInteractionType.DragUpdate);
            return;
        }

        var objToProcess = hit.collider.GetComponent<IGameViewObject>();
        ProcessGameViewObject(objToProcess, InputInteractionType.DragUpdate);
    }

    private void OnDragCanceled(InputAction.CallbackContext obj)
    {
        if (!isDragging)
            return;

        if (!TryGetHit(out var hit))
        {
            ProcessGameViewObject(null, InputInteractionType.EndDrag);
            return;
        }

        var gameViewObject = hit.collider.GetComponent<IGameViewObject>();
        ProcessGameViewObject(gameViewObject, InputInteractionType.EndDrag);
    }

    private void ProcessGameViewObject(IGameViewObject gameViewObject, InputInteractionType interactionType)
    {
        if (_gameView3D == null)
        {
            Debug.Log("gameView not set");
            return;
        }

        if (!TryResolveWorldPosition(gameViewObject, out var worldPosition))
        {
            worldPosition = Vector3.zero;
        }

        switch (interactionType)
        {
            case InputInteractionType.Hover:
                if (gameViewObject != null)
                {
                    _gameView3D?.HandleGameViewObjectHovered(gameViewObject);
                }
                break;
            case InputInteractionType.Select:
                if (gameViewObject != null)
                {
                    _gameView3D?.HandleGameViewObjectSelected(gameViewObject);
                }
                break;
            case InputInteractionType.Action:
                if (gameViewObject != null)
                {
                    _gameView3D?.HandleActionPerformed(gameViewObject);
                }
                else
                {
                    ActionCanceled?.Invoke();
                }
                break;
            case InputInteractionType.BeginDrag:
                var unitView = ResolveUnitView(gameViewObject);
                if (unitView != null)
                {
                    isDragging = true;
                    _gameView3D?.BeginDrag(unitView);
                }
                break;
            case InputInteractionType.DragUpdate:
                if (!isDragging)
                {
                    break;
                }

                if (!TryResolveWorldPosition(gameViewObject, out var dragPosition))
                {
                    dragPosition = Vector3.zero;
                }

                _gameView3D?.UpdateDrag(dragPosition);
                break;
            case InputInteractionType.EndDrag:
                if (!isDragging)
                {
                    break;
                }

                isDragging = false;
                if (TryResolveWorldPosition(gameViewObject, out var endPosition))
                {
                    _gameView3D?.EndDrag(endPosition);
                }
                else
                {
                    ActionCanceled?.Invoke();
                }
                break;
        }
    }

    private UnitView3D ResolveUnitView(IGameViewObject gameViewObject)
    {
        if (gameViewObject is UnitView3D unitView)
        {
            return unitView;
        }

        if (gameViewObject is Component component)
        {
            return component.GetComponentInParent<UnitView3D>();
        }

        return null;
    }

    private bool TryResolveWorldPosition(IGameViewObject gameViewObject, out Vector3 worldPosition)
    {
        if (gameViewObject is Component component)
        {
            worldPosition = component.transform.position;
            return true;
        }

        worldPosition = default;
        return false;
    }
    private bool TryGetHit(out RaycastHit hit)
    {
        var cam = mainCamera != null ? mainCamera : Camera.main;
        if (cam == null)
        {
            hit = default;
            return false;
        }

        var ray = cam.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit, 999f, mouseColliderLayerMask);
        var result = hit.collider != null;
        return result;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        var eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        uiRaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, uiRaycastResults);
        return uiRaycastResults.Count > 0;
    }
    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null || Mouse.current == null)
        {
            return Vector3.zero;
        }

        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }
        return Vector3.zero;
    }

#if UNITY_INCLUDE_TESTS
    public void TestSetGameView(GameView3D view) => _gameView3D = view;
    public void TestSetSelectionMode(SelectionMode mode) => selectionMode = mode;
    public void TestProcessGameViewObject(IGameViewObject obj, InputInteractionType interaction) => ProcessGameViewObject(obj, interaction);
    public void TestSetCamera(Camera camera) => mainCamera = camera;
    public void TestSetTurnState(ITurnStateViewModel turnState)
    {
        _battleStateSubscription?.Dispose();
        _turnState = turnState;
        _battleStateSubscription = turnState?.BattleStateProperty.Subscribe(OnTurnStateChanged);
    }
#endif
}
