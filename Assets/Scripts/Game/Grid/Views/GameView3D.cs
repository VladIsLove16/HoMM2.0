using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private List<Vector2Int> currentReachableCells = new();
    [SerializeField] private List<(Vector2Int,bool)> currentRoutePoints= new();
    [SerializeField] bool cellSelectionAvailable;
    [Inject]
    public void Construct(GameViewModel gameGridViewModel, InputSystem_Actions inputActions)
    {
        Debug.Log("GameView3D is ready");
        _gameViewModel = gameGridViewModel;
        gameGridViewModel.CellContentAdded += GameGridViewModel_OnCellContentAdded;
        gameGridViewModel.CellContentRemoved += GameGridViewModel_OnCellContentRemoved;
        gameGridViewModel.CellContentSwaped += GameGridViewModel_OnCellContentSwapped;
        gameGridViewModel.CellContentMoved += GameGridViewModel_OnCellContentMoved;
        gameGridViewModel.CellContentMovedByRoute += GameGridViewModel_OnCellContentMovedByRoute;
        gameGridViewModel.ReachableCellsChanged += GameGridViewModel_OnReachableCellsChanged;
        gameGridViewModel.CellSelected += GameGridViewModel_OnCellSelected;
        gameGridViewModel.RoutePointsChanged += GameGridViewModel_OnRoutePointsChanged;
        _inputActions = inputActions;
        _inputActions.Enable();
        _inputActions.Grid.Select.performed += OnSelectPerformed;
        _inputActions.Grid.Action.performed += OnActionPerformed;
        _inputActions.Grid.MousePosition.performed += OnMousePositionChanged;
    }

    private void GameGridViewModel_OnRoutePointsChanged(List<(Vector2Int,bool)> list)
    {
        Debug.Log(currentRoutePoints.Count);
        Debug.Log(list.Count);
        foreach (var point in currentRoutePoints)
        {
            Debug.Log("removing " + point);
            _gridCellRenderer.RemoveState(point.Item1, point.Item2 ? CellState.accessibleRoutePoint : CellState.inaccessibleRoutePoint);
        }
        foreach (var point in list) 
        {
            Debug.Log("adding " + point);
            _gridCellRenderer.AddState(point.Item1, point.Item2 ? CellState.accessibleRoutePoint : CellState.inaccessibleRoutePoint);
        }
        currentRoutePoints = list.ToList();

    }

    private void GameGridViewModel_OnCellSelected(Vector2Int coords)
    {
        selectedCell = coords;
        Select(coords);
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
    private IEnumerator PlayMoveAnimation(List<Vector3> to, UnitView3D unit)
    { 
        Debug.Log("Playing MoveAnimation");
        foreach (var item in to)
        {
            float time = 0f;
            Vector3 fromPos = unit.transform.position;
            while (time < movingTime)
            {
                time += Time.deltaTime;
                unit.transform.position = Vector3.Lerp(fromPos, item, time / movingTime);
                yield return null;
            }
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
    private void GameGridViewModel_OnCellContentMovedByRoute(UnitModel model, List<Vector2Int> route)
    {
        UnitView3D unitView3D = views[model];
        List<Vector3> worldPoints = route.Select(point => _gridCellRenderer.ToWorld(point.x, point.y)).ToList();
        StartCoroutine(PlayMoveAnimation(worldPoints, unitView3D));
    }
    private void GameGridViewModel_OnReachableCellsChanged(List<Vector2Int> list)
    {
        ShowReachableCells(list);
    }

    public void ShowReachableCells(List<Vector2Int> moveableCells)
    {
        foreach(var a in currentReachableCells)
        {
            _gridCellRenderer.RemoveState(a, CellState.moveAvailable);
        }
        foreach (Vector2Int cellCoords in moveableCells)
        {
            _gridCellRenderer.AddState(cellCoords, CellState.moveAvailable);
        }
        currentReachableCells = moveableCells;
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
        Debug.Log("SetSelected performed");
        if(!cellSelectionAvailable)
        {
            Debug.Log("cellSelection is not available");
            return; 
        }
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
            _gameViewModel.PerformAction(hoveredCell);
            Select(hoveredCell);
        }
        else
            Debug.Log("no cell hovered");
    }

    private void OnMousePositionChanged(InputAction.CallbackContext context)
    {
        bool hit = Mouse3D.GetMouseWorldPosition(out Vector3 worldPos);
        if (!hit)
        {
            return;
        }
        bool success = _gridCellRenderer.ToGrid(worldPos, out Vector2Int coords);
        if ((success))
        {
            HoverCell(coords);
        }
        else
        {
            isCellHovered = false;
        }
    }

    private void Select(Vector2Int coords)
    {
        if (selectedCell == coords)
            return;
        if (isCellSelected)
        {
            _gridCellRenderer.RemoveState(selectedCell,CellState.selected);
        }

        selectedCell = coords;
        isCellSelected = true;

        _gridCellRenderer.AddState(coords, CellState.selected);
    }

    private void HoverCell(Vector2Int coords)
    {
        if (coords == hoveredCell)
            return;
        if (isCellHovered)
        {
            _gridCellRenderer.RemoveState(hoveredCell,CellState.hovered);
        }

        hoveredCell = coords;
        isCellHovered = true;

        _gridCellRenderer.AddState(coords, CellState.hovered);
        _gameViewModel.HandleCellHovered(coords);
    }
}
