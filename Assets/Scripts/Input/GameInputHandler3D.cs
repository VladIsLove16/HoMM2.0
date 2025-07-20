using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class GameInputHandler3D : MonoBehaviour
{
	[Inject] private IGridCellRenderer _renderer;
	[Inject] private InputSystem_Actions _inputActions;

	public event Action<Vector2Int> CellHovered;
	public event Action<Vector2Int> CellSelected;
	public event Action ActionRequested;

	public Vector2Int CurrentHoveredCell { get; private set; }

	private void Start()
	{
		_inputActions.Enable();
		_inputActions.Grid.MousePosition.performed += OnMouseMoved;
		_inputActions.Grid.Select.performed += OnCellSelected;
		_inputActions.Grid.Action.performed += OnAction;
	}

	private void OnMouseMoved(InputAction.CallbackContext context)
	{
		if (Mouse3D.GetMouseWorldPosition(out Vector3 worldPos) &&
			_renderer.ToGrid(worldPos, out Vector2Int coords))
		{
			CurrentHoveredCell = coords;
			CellHovered?.Invoke(coords);
		}
	}

	private void OnCellSelected(InputAction.CallbackContext context)
	{
		CellSelected?.Invoke(CurrentHoveredCell);
	}

	private void OnAction(InputAction.CallbackContext context)
	{
		ActionRequested?.Invoke();
	}
}
