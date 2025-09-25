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
    [Inject] private TurnSystem turnSystem;
    private readonly System.Collections.Generic.List<RaycastResult> uiRaycastResults = new System.Collections.Generic.List<RaycastResult>(8);
    private Collider _lastHoveredCollider;
    public Action ActionCanceled; // optional external consumer
    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (inputActions == null) inputActions = new InputSystem_Actions();
        turnSystem.BattleStateChanged += OnTurnSystem_BattleStateChanged;

        inputActions.Grid.MousePosition.performed += OnMouseMoved;
        inputActions.Grid.Select.performed += OnSelectPerformed;
        inputActions.Grid.Action.performed += OnActionPerformed;

        inputActions.GridPlacement.Drag.started += OnDragStarted;
        inputActions.GridPlacement.Drag.performed += OnDragPerformed;
        inputActions.GridPlacement.Drag.canceled += OnDragCanceled;
    }

    private void OnTurnSystem_BattleStateChanged(BattleState state)
    {
        switch(state)
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
        inputActions.Grid.MousePosition.performed -= OnMouseMoved;
        inputActions.Grid.Select.performed -= OnSelectPerformed;
        inputActions.Grid.Action.performed -= OnActionPerformed;
        inputActions.GridPlacement.Drag.started -= OnDragStarted;
        inputActions.GridPlacement.Drag.performed -= OnDragPerformed;
        inputActions.GridPlacement.Drag.canceled -= OnDragCanceled;
        inputActions.Disable();
    }

    private void OnMouseMoved(InputAction.CallbackContext ctx)
    {
        if (TryGetHit(out var hit))
        {
            Debug.Log("hit " + hit.collider.gameObject.name);
            if (_lastHoveredCollider == hit.collider)
            {
                Debug.Log("(_lastHoveredCollider == hit.collider)");
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
                // Сначала пытаемся найти юнит
                gameViewObject = hit.collider != null ? hit.collider.GetComponent<IGameViewObject>() : null;
                if (gameViewObject == null)
                {
                    // Если юнит не найден, ищем клетку
                    gameViewObject = hit.collider != null ? hit.collider.GetComponent<IGameViewObject>() : null;
                }
            }
            else if (selectionMode == SelectionMode.GridOnly)
            {
                // Только клетки
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
            
            // Определяем, что выбрано в зависимости от режима
            if (selectionMode == SelectionMode.UnitThenGrid)
            {
                // Сначала пытаемся найти юнит
                gameViewObject = hit.collider != null ? hit.collider.GetComponentInParent<IGameViewObject>() : null;
                if (gameViewObject == null)
                {
                    // Если юнит не найден, ищем клетку
                    gameViewObject = hit.collider != null ? hit.collider.GetComponentInParent<IGameViewObject>() : null;
                }
            }
            else if (selectionMode == SelectionMode.GridOnly)
            {
                // Только клетки
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
        if (TryGetHit(out var hit))
        {
            var unitView = hit.collider.GetComponent<UnitView3D>();
            if (unitView != null)
                _gameView3D.BeginDrag(unitView);
        }
    }

    private void OnDragPerformed(InputAction.CallbackContext ctx)
    {
        var worldPos = GetMouseWorldPosition();
        _gameView3D.UpdateDrag(worldPos);
    }

    private void OnDragCanceled(InputAction.CallbackContext ctx)
    {
        if (TryGetHit(out var hit))
        {
            _gameView3D.EndDrag(hit.point);
        }
    }


    private bool TryGetHit(out RaycastHit hit)
    {
        var cam = mainCamera != null ? mainCamera : Camera.main;
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit, 999f, mouseColliderLayerMask);
        return hit.collider != null;
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
        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }
        return Vector3.zero;
    }
}


