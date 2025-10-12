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
    [Inject] private TurnSystem _turnSystem;
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
    public void Construct(TurnSystem turnSystem)
    {
        _turnSystem = turnSystem;
        EnsureInputActionsInitialized();
        _turnSystem.CurrentBattleState.Subscribe(OnTurnSystem_BattleStateChanged);
        OnTurnSystem_BattleStateChanged(_turnSystem.CurrentBattleState.Value);
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

        if (_turnSystem != null)
        {
            OnTurnSystem_BattleStateChanged(_turnSystem.CurrentBattleState.Value);
        }
        else
        {
            inputActions.Grid.Enable();
            inputActions.GridPlacement.Disable();
        }
    }

    private void OnTurnSystem_BattleStateChanged(BattleState state)
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

    private void OnSelectPerformed(InputAction.CallbackContext ctx)
    {
        if (IsPointerOverUI()) return;
        
        if (TryGetHit(out var hit))
        {
            IGameViewObject gameViewObject = null;
            if (selectionMode == SelectionMode.UnitThenGrid)
            {
                gameViewObject = hit.collider != null ? hit.collider.GetComponent<IGameViewObject>() : null;
                if (gameViewObject == null)
                {
                    gameViewObject = hit.collider != null ? hit.collider.GetComponent<IGameViewObject>() : null;
                }
            }
            else if (selectionMode == SelectionMode.GridOnly)
            {
                gameViewObject = hit.collider != null ? hit.collider.GetComponent<IGameViewObject>() : null;
            }
            
            if (gameViewObject != null && gameViewObject.IsSelectable)
            {
                _gameView3D?.HandleGameViewObjectSelected(gameViewObject);
            }
        }
    }

    private void OnActionPerformed(InputAction.CallbackContext ctx)
    {
        if (IsPointerOverUI()) return;
        
        if (TryGetHit(out var hit))
        {
            IGameViewObject gameViewObject = null;
            
            if (selectionMode == SelectionMode.UnitThenGrid)
            {
                // РЎРЅР°С‡Р°Р»Р° РїС‹С‚Р°РµРјСЃСЏ РЅР°Р№С‚Рё СЋРЅРёС‚
                gameViewObject = hit.collider != null ? hit.collider.GetComponentInParent<IGameViewObject>() : null;
                if (gameViewObject == null)
                {
                    gameViewObject = hit.collider != null ? hit.collider.GetComponentInParent<IGameViewObject>() : null;
                }
            }
            else if (selectionMode == SelectionMode.GridOnly)
            {
                gameViewObject = hit.collider != null ? hit.collider.GetComponentInParent<IGameViewObject>() : null;
            }
            
            if (gameViewObject != null)
            {
                _gameView3D?.HandleActionPerformed(gameViewObject);
            }
        }
        else
        {
            ActionCanceled?.Invoke();
        }
    }
    private void OnDragStarted(InputAction.CallbackContext ctx)
    {
        Debug.Log("Drag started");  
        if (TryGetHit(out var hit))
        {
            Debug.Log(hit.collider.gameObject.name);
            var unitView = hit.collider.GetComponent<UnitView3D>();
            if (unitView != null)
            {
                isDragging = true;
                _gameView3D.BeginDrag(unitView);
            }
        }
    }

    private void OnDragPerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log("OnDragPerformed");
        var worldPos = GetMouseWorldPosition();
        _gameView3D.UpdateDrag(worldPos);
    }

    private void OnDragCanceled(InputAction.CallbackContext ctx)
    {
        Debug.Log("OnDragCanceled");
        if (TryGetHit(out var hit))
        {
            if (!isDragging)
                return;
            isDragging = false;
            _gameView3D.EndDrag(hit.point);
        }
    }


    private void ProcessGameViewObject(IGameViewObject gameViewObject, InputInteractionType interaction)
    {
        switch (interaction)
        {
            case InputInteractionType.Hover:
                if (gameViewObject?.IsHoverable == true)
                {
                    _gameView3D?.HandleGameViewObjectHovered(gameViewObject);
                }
                break;
            case InputInteractionType.Select:
                if (gameViewObject?.IsSelectable == true)
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
#endif
}