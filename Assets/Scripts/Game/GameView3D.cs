using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameView3D : MonoBehaviour
{
    private const float CORPSEALIVETIME = 5f;
    private GameViewModel _gameGridViewModel;
    [Inject]  private UnitViewFactory unitViewFactory;
    Dictionary<UnitViewModel, UnitView3D> views = new();
    [SerializeField] private float movingTime = 1.5f;
    [Inject] private ICellGridRenderer _cellGridRenderer;

    [Inject]
    public void Construct(GameViewModel gameGridViewModel)
    {
        Debug.Log("GameView3D is ready");
        this._gameGridViewModel = gameGridViewModel;
        gameGridViewModel.OnCellContentAdded += GameGridViewModel_OnCellContentAdded;
        gameGridViewModel.OnCellContentRemoved += GameGridViewModel_OnCellContentRemoved;
        gameGridViewModel.OnCellContentSwaped += GameGridViewModel_OnCellContentSwapped;
        gameGridViewModel.OnCellContentMoved += GameGridViewModel_OnCellContentMoved;
        gameGridViewModel.OnTurnStarted += GameGridViewModel_OnTurnStarted;
    }


    private void GameGridViewModel_OnCellContentSwapped(UnitViewModel toModel,UnitViewModel fromModel)
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
    private void GameGridViewModel_OnCellContentAdded(UnitViewModel viewModel)
    {
        Debug.Log("unitView3D creating in " + viewModel.X + " " + viewModel.Y);
        UnitView3D unitView3D = unitViewFactory.Create(viewModel);
        if (unitView3D != null)
        {
            views[viewModel] = unitView3D;
        }
    }

    private void GameGridViewModel_OnCellContentRemoved(UnitViewModel viewModel)
    {
        UnitView3D unitView3D = views[viewModel];
        views.Remove(viewModel);
        Destroy(unitView3D, CORPSEALIVETIME);
    }

    private void GameGridViewModel_OnCellContentMoved(UnitViewModel model, Vector2Int to)
    {
        UnitView3D unitView3D = views[model];
        Vector3 toPos = _cellGridRenderer.ToWorld(to.x,to.y);
        StartCoroutine(PlayMoveAnimation(toPos, unitView3D));
    }
    private void GameGridViewModel_OnTurnStarted(UnitViewModel unitViewModel)
    {
        UnitView3D unitView3D = views[unitViewModel];
        _gameGridViewModel.GetAvailableMoves();
        List<Vector2Int> moveableCells = unitViewModel.GetMoveableCells();
        ShowAvailableMoves(moveableCells);
        // unitView3D.OnTurnStarted;
    }

    private void ShowAvailableMoves(List<Vector2Int> moveableCells)
    {
        foreach (Vector2Int cellCoords in moveableCells)
        {
            _cellGridRenderer.SetCellState(cellCoords, CellState.moveAvailable);
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
}

