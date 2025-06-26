using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using static UnityEditor.Profiling.HierarchyFrameDataView;

public class GameView3D : MonoBehaviour
{
    private const float CORPSEALIVETIME = 5f;
    private GameViewModel _gameViewModel;
    [Inject]  private UnitViewFactory unitViewFactory;
    Dictionary<UnitModel, UnitView3D> views = new();
    [SerializeField] private float movingTime = 1.5f;
    [Inject] private IGridCellRenderer _gridCellRenderer;
    private InputSystem_Actions _inputActions;
    private Vector2Int hoveredCell;
    private Vector2Int selectedCell;
    private bool isCellHovered;
    private bool isCellSelected;

    [Inject]
    public void Construct(GameViewModel gameGridViewModel, InputSystem_Actions inputActions)
    {
        Debug.Log("GameView3D is ready");
        _gameViewModel = gameGridViewModel;
        gameGridViewModel.OnCellContentAdded += GameGridViewModel_OnCellContentAdded;
        gameGridViewModel.OnCellContentRemoved += GameGridViewModel_OnCellContentRemoved;
        gameGridViewModel.OnCellContentSwaped += GameGridViewModel_OnCellContentSwapped;
        gameGridViewModel.OnCellContentMoved += GameGridViewModel_OnCellContentMoved;
        gameGridViewModel.OnReachableCellsChanged += GameGridViewModel_OnReachableCellsChanged;

        _inputActions = inputActions;
        _inputActions.Enable();
        _inputActions.Grid.Select.performed += OnSelectPerformed;
        _inputActions.Grid.Action.performed += OnActionPerformed;
        _inputActions.Grid.MousePosition.performed += OnMousePositionChanged;
    }


    private void GameGridViewModel_OnCellContentSwapped(UnitModel toModel, UnitModel fromModel)
    {
        Debug.Log("PlaySwapAnimation");
        UnitView3D toUnitView3D = views[toModel];
        UnitView3D fromUnitView3D = views[fromModel];
        StartCoroutine(PlaySwapAnimation(toUnitView3D, fromUnitView3D));
    }

    private IEnumerator PlaySwapAnimation(UnitView3D toUnitView3D, UnitView3D fromUnitView3D)
    {
        float time = 0f;
        Debug.Log("animation time" + time);
        Vector3 toPos = toUnitView3D.transform.position;
        Vector3 fromPos = fromUnitView3D.transform.position;
        while (time < movingTime)
        {
            time+= Time.deltaTime;
            Debug.Log("animation time" + time);
            toUnitView3D.transform.position = Vector3.Lerp(toPos, fromPos, time / movingTime);
            fromUnitView3D.transform.position =  Vector3.Lerp(fromPos, toPos, time / movingTime);
            yield return null;
        }
    }
    private IEnumerator PlayMoveAnimation(Vector3 to, UnitView3D unit)
    {
        float time = 0f;
        Debug.Log("Playing MoveAnimation");
        Vector3 fromPos = unit.transform.position;
        while (time < movingTime)
        {
            time += Time.deltaTime;
            unit.transform.position = Vector3.Lerp(fromPos, to , time / movingTime);
            yield return null;
        }
    }
    private void GameGridViewModel_OnCellContentAdded(UnitModel model)
    {
        Debug.Log("unitView3D creating in " + model.X + " " + model.Y);
        UnitView3D unitView3D = unitViewFactory.Create(model);
        if (unitView3D != null)
        {
            views[model] = unitView3D;
        }
    }

    private void GameGridViewModel_OnCellContentRemoved(UnitModel viewModel)
    {
        UnitView3D unitView3D = views[viewModel];
        views.Remove(viewModel);
        Destroy(unitView3D, CORPSEALIVETIME);
    }

    private void GameGridViewModel_OnCellContentMoved(UnitModel model, Vector2Int to)
    {
        UnitView3D unitView3D = views[model];
        Vector3 toPos = _gridCellRenderer.ToWorld(to.x,to.y);
        StartCoroutine(PlayMoveAnimation(toPos, unitView3D));
    }

    private void GameGridViewModel_OnReachableCellsChanged(List<Vector2Int> list)
    {
        ShowReachableCells(list);
    }

    public void ShowReachableCells(List<Vector2Int> moveableCells)
    {
        foreach (Vector2Int cellCoords in moveableCells)
        {
            _gridCellRenderer.SetCellState(cellCoords, CellState.moveAvailable);
        }
    }

    private void Destroy(UnitView3D unitView3D,float time = 0f)
    {
        Debug.Log("content will be removed after" + time);
        if (unitView3D != null)
            GameObject.Destroy(unitView3D.gameObject);
        else
            Debug.LogWarning("_gridView have been destroyed");
    }

    private void OnSelectPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Select performed");
        bool hitCollider = Mouse3D.GetMouseWorldPosition(out Vector3 worldPos);
        if (!hitCollider)
        {
            Debug.Log("not hit found");
            return;
        }
        bool success = _gridCellRenderer.ToGrid(worldPos, out Vector2Int coords);
        if ((success))
        {
            Debug.Log($"Cell selected {coords.x} {coords.y} in {worldPos}");

            Select(coords);

            _gameViewModel.HandleCellSelected(coords);
        }
        else
            Debug.Log("cant get cell in " + worldPos);
    }

    private void OnActionPerformed(InputAction.CallbackContext context)
    {
        if (isCellHovered)
        {
            _gameViewModel.HandleActionPerformed(hoveredCell);
            Select(hoveredCell);
        }
        else
            Debug.Log("no cell hovered");
    }

   

    private void OnMousePositionChanged(InputAction.CallbackContext context)
    {
        bool hitCollider = Mouse3D.GetMouseWorldPosition(out Vector3 worldPos);
        if (!hitCollider)
        {
            return;
        }
        bool success = _gridCellRenderer.ToGrid(worldPos, out Vector2Int coords);
        if ((success))
        {
            Hover(coords);

            _gameViewModel.HandleCellHovered(coords);

        }
        else
            isCellHovered = false;
    }

    private void Select(Vector2Int coords)
    {
        if (selectedCell == coords)
            return;
        if (isCellSelected)
            _gridCellRenderer.SetCellState(selectedCell, CellState.normal);

        selectedCell = coords;
        isCellSelected = true;

        _gridCellRenderer.SetCellState(coords, CellState.selected);
    }

    private void Hover(Vector2Int coords)
    {
        if (coords == hoveredCell)
            return;
        if (isCellHovered)
        {
            CellState prevState = _gridCellRenderer.GetPrevState(hoveredCell);
            _gridCellRenderer.SetCellState(hoveredCell, prevState);
            Debug.Log("cell hovered" +  coords);
        }

        hoveredCell = coords;
        isCellHovered = true;

        _gridCellRenderer.SetCellState(coords, CellState.hovered);
    }
}

