using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

// Publishes high-level input signals (hover, select, action) in grid coordinates.
// Knows nothing about turns, rendering, or game rules.
public class CellInputHandler
{
    [SerializeField] private LayerMask mouseColliderLayerMask;
    public enum SelectionMode { GridOnly, UnitThenGrid }
    [SerializeField] private SelectionMode selectionMode = SelectionMode.UnitThenGrid;

    private InputSystem_Actions inputActions;

    [SerializeField] private Camera mainCamera;
    [Inject] private GameView3D _gameView3D;
    private readonly System.Collections.Generic.List<RaycastResult> uiRaycastResults = new System.Collections.Generic.List<RaycastResult>(8);

    public Action ActionCanceled; // optional external consumer

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (inputActions == null) inputActions = new InputSystem_Actions();
        inputActions.Enable();
        inputActions.Grid.MousePosition.performed += OnMouseMoved;
        inputActions.Grid.Select.performed += OnSelectPerformed;
        inputActions.Grid.Action.performed += OnActionPerformed;
    }

    private void OnDisable()
    {
        inputActions.Grid.MousePosition.performed -= OnMouseMoved;
        inputActions.Grid.Select.performed -= OnSelectPerformed;
        inputActions.Grid.Action.performed -= OnActionPerformed;
        inputActions.Disable();
    }

    private void OnMouseMoved(InputAction.CallbackContext ctx)
    {
        if (TryGetHit(out var hit))
        {
            var gameViewObject = hit.collider != null ? hit.collider.GetComponentInParent<IGameViewObject>() : null;
            if (gameViewObject != null && gameViewObject.IsHoverable)
            {
                // Notify GameView3D about hovered object
                _gameView3D?.HandleGameViewObjectHovered(gameViewObject);
            }
        }
    }

    private void OnSelectPerformed(InputAction.CallbackContext ctx)
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
            
            if (gameViewObject != null && gameViewObject.IsSelectable)
            {
                // Notify GameView3D about selected object
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

    private bool TryGetHit(out RaycastHit hit)
    {
        var cam = mainCamera != null ? mainCamera : Camera.main;
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hit, 999f, mouseColliderLayerMask);
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        var eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        uiRaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, uiRaycastResults);
        return uiRaycastResults.Count > 0;
    }
}


