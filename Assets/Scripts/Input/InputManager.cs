using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class InputManager
{
    InputSystem_Actions _inputActions = new();
    [Inject] GameController _gameController;
    GridView _gridView;
    private Vector2Int hoveredCell;
    private bool isCellHovered;
    [Inject]
    public InputManager(GridView gridView)
    {
        _gridView = gridView;
        _inputActions.Enable();
        _inputActions.Grid.Select.performed += OnSelectPerformed;
        _inputActions.Grid.Action.performed += OnActionPerformed;
        _inputActions.Grid.MousePosition.performed += OnMousePositionChanged;
        Debug.Log("Input manager ready");
    }


    private void OnSelectPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Select performed");
        bool hitCollider = Mouse3D.GetMouseWorldPosition(out Vector3 worldPos);
        if(!hitCollider)
        {
            Debug.Log("not hit found");
            return;
        }
        bool success = _gridView.GetCell(worldPos, out Vector2Int coords);
        if ((success))
        {
            Debug.Log($"Cell selected {coords.x} {coords.y} in {worldPos}");
            _gameController.SelectCell(coords);
        }
        else
            Debug.Log("cant get cell in " + worldPos);
    }

    private void OnActionPerformed(InputAction.CallbackContext context)
    {
        if(isCellHovered)
            _gameController.PerformAction(hoveredCell);
        else
            Debug.Log("no cell hovered");
    }


    private void OnMousePositionChanged(InputAction.CallbackContext context)
    {
        bool hitCollider = Mouse3D.GetMouseWorldPosition(out Vector3 worldPos);
        if (!hitCollider)
        {
            Debug.Log("not hit found");
            return;
        }
        bool success = _gridView.GetCell(worldPos, out Vector2Int coords);
        if ((success))
        {
            Debug.Log($"Cell selected {coords.x} {coords.y} in {worldPos}");
            hoveredCell = coords;
            isCellHovered = true;
            _gameController.Hover(coords);
        }
    }
}
