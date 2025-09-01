using System;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.EventSystems; // Добавляем для проверки UI
using Zenject;

public class GameInputHandler3D : MonoBehaviour
{
    [SerializeField] private LayerMask mouseColliderLayerMask;
    private InputSystem_Actions _inputActions;
    [Inject] private IWorldToCellProvider _renderer;
    public ReactiveProperty<Vector2Int> HoveredCell;
    public ReactiveProperty<Vector2Int> SelectedCell;
    public ReactiveProperty<Vector2Int> ActionPerformed;
    public Action ActionCanceled;
    public ReactiveProperty<Collider> HoveredCollider;
    public ReactiveProperty<Vector3> WorldMousePosition;
    
    private void OnEnable()
    {
        if(_inputActions == null)
            _inputActions = new();
        _inputActions.Enable();
        _inputActions.Grid.MousePosition.performed += OnMouseMoved;
        _inputActions.Grid.Select.performed += OnSelectPerformed;
        _inputActions.Grid.Action.performed += OnActionPerformed;
    }

    private void OnDisable()
    {
        _inputActions.Grid.MousePosition.performed -= OnMouseMoved;
        _inputActions.Grid.Select.performed -= OnSelectPerformed;
        _inputActions.Grid.Action.performed -= OnActionPerformed;
        _inputActions.Disable();
    }

    private void OnMouseMoved(InputAction.CallbackContext ctx)
    {
        if (GetHit(out RaycastHit raycastHit,out Ray ray))
        {
            if(raycastHit.collider != HoveredCollider.Value)
                HoveredCollider.SetValueAndForceNotify(raycastHit.collider);
            if (raycastHit.point != WorldMousePosition.Value)
                WorldMousePosition.SetValueAndForceNotify(raycastHit.point);
            if(_renderer.ToGrid(raycastHit.point, out Vector2Int coords))
            {
                if(coords != HoveredCell.Value)
                    HoveredCell.SetValueAndForceNotify(coords);
            }
        }
    }
    
    private bool GetHit(out RaycastHit raycastHit, out Ray ray)
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out raycastHit, 999f, mouseColliderLayerMask);
    }
    
    private void OnSelectPerformed(InputAction.CallbackContext ctx)
    {
        // Сначала проверяем, не кликнули ли по UI
        if (IsPointerOverUI())
            return; // Если кликнули по UI - игнорируем игровую логику

        if(GetHit(out var raycastHit,out var ray))
        {
            if (_renderer.ToGrid(raycastHit.point, out Vector2Int coords))
                SelectedCell.SetValueAndForceNotify(coords);
        }
    }
    
    private void OnActionPerformed(InputAction.CallbackContext ctx)
    {
        // Сначала проверяем, не кликнули ли по UI
        if (IsPointerOverUI())
            return; // Если кликнули по UI - игнорируем игровую логику

        if(GetHit(out var raycastHit, out var ray))
        {
            if (_renderer.ToGrid(raycastHit.point, out Vector2Int coords))
                ActionPerformed.SetValueAndForceNotify(coords);
        }
        else
            ActionCanceled?.Invoke();
    }

    // Метод для проверки, находится ли указатель над UI элементом
    private bool IsPointerOverUI()
    {
        // Проверяем EventSystem
        if (EventSystem.current == null)
            return false;
        
        // Используем GraphicRaycaster для надежной проверки UI
        var eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        // Если есть UI элементы под курсором - блокируем игровые действия
        return results.Count > 0;
    }
}
