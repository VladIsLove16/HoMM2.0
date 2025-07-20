using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// ViewModel: связывает GameModel и представления, обрабатывает команды и генерирует готовые к отображению данные.
/// </summary>
public class GameViewModel : IInitializable, IDisposable
{
    private readonly GameModel _gameModel;

    public event Action<string> Error;
    public event Action<string> LastChangedCellDescription;
    //public event Action<string> OnSelectedCellDescription;

    public Action<UnitModel> CellContentAdded;
    public Action<UnitModel> CellContentRemoved;
    public Action<UnitModel, UnitModel> CellContentSwaped;
    public Action<UnitModel, Vector2Int> CellContentMoved;
    public Action<UnitModel, List<Vector2Int>> CellContentMovedByRoute;
    public Action<List<Vector2Int>> ReachableCellsChanged;
    public Action<List<(Vector2Int,bool)>> RoutePointsChanged;
    public Action<UnitModel> OnTurnStarted;
    public Action<Vector2Int> CellSelected;

    private Vector2Int selectedCell;
    private bool isCellSelected;

    private readonly ICursorService _cursorService;
    public GameViewModel(GameModel model, ICursorService cursorService)
    {
        _gameModel = model;
        _cursorService = cursorService;
        Initialize();
    }

    public void Initialize()
    {
        _gameModel.CellContentAdded += HandleContentAdded;
        _gameModel.CellContentRemoved += HandleContentRemoved;
        _gameModel.CellContentSwaped += HandleContentSwaped;
        _gameModel.CellContentMoved += HandleContentMoved;
        _gameModel.CellContentMovedByRoute += HandleContentMovedByRoute;
        _gameModel.TurnStarted += HandleTurnStarted;

        _cursorService.SetCursorVisibility(true);
    }

    private void HandleContentMovedByRoute(UnitModel model, List<Vector2Int> list)
    {
        CellContentMovedByRoute?.Invoke(model, list);
    }

    public void Dispose()
    {
        _gameModel.CellContentAdded -= HandleContentAdded;
        _gameModel.CellContentRemoved -= HandleContentRemoved;
        _gameModel.CellContentSwaped -= HandleContentSwaped;
        _gameModel.CellContentMoved -= HandleContentMoved;
    }

    public int GetWidth() => _gameModel.GetWidth();
    public int GetHeight() => _gameModel.GetHeight();
    public IReadOnlyList<IGridCell> GetCells() => _gameModel.GetAllCells();

    /// <summary>
    /// Спавнит юнит, парсит координаты и вызывает модель.
    /// </summary>
    public OperationResult SpawnUnitAt(string xRaw, string yRaw, UnitType type = 0)
    {
        if (!int.TryParse(xRaw, out var x) || !int.TryParse(yRaw, out var y))
        {
            Error?.Invoke("Координаты должны быть числами");
            return new OperationResult(false, "Invalid coordinates");
        }
        UnitSpawnParams @params = new(x, y, type);
        var result = _gameModel.SpawnUnit(@params);
        if (!result.IsSuccess)
            Error?.Invoke(result.Message);
        return result;
    }

    /// <summary>
    /// Спавнит юнит в случайной пустой клетке.
    /// </summary>
    public void SpawnUnitAtRandomPlace()
    {
        var (x, y) = _gameModel.GetEmpty();
        UnitSpawnParams @params = new(x, y);
        var result = _gameModel.SpawnUnit(@params);
        if (!result.IsSuccess)
            Error?.Invoke(result.Message);
    }

    ///// <summary>
    ///// Выбрать клетку и получить её описание.
    ///// </summary>
    //public OperationResult GetCell(string xRaw, string yRaw, out )
    //{
    //    try
    //    {
    //        var cell = _gameModel.GetCell(x, y);
    //        OnSelectedCellDescription?.Invoke(cell.ToString());
    //        return new OperationResult()
    //        {
    //            IsSuccess = true,
    //            Message = "GetCell success"
    //        };
    //    }
    //    catch (Exception ex)
    //    {
    //        Error?.Invoke(ex.Message);
    //        return new OperationResult()
    //        {
    //            Message = ex.Message,
    //            IsSuccess = false
    //        };
    //    }
    //}

    private void HandleContentAdded(UnitModelCreatedParams e)
    {
        CellContentAdded?.Invoke(e.UnitModel);
        var desc = _gameModel.GetCell(e.UnitModel.X, e.UnitModel.Y).ToString();
        LastChangedCellDescription?.Invoke(desc);
    }

    private void HandleContentRemoved(UnitModelRemovedParams e)
    {
        Debug.Log("content removed");
        CellContentRemoved?.Invoke(e.UnitModel);
    }
    private void HandleContentSwaped(UnitModelsSwapped e)
    {
        Debug.Log(" HandleContentSwaped(UnitModelsSwapped swapped)");
        CellContentSwaped?.Invoke(e.from, e.to);
    }
    private void HandleContentMoved(UnitModel model, Vector2Int to)
    {
        Debug.Log(" CellContentMoved?.Invoke(viewModel, to);");
        CellContentMoved?.Invoke(model, to);
    }

    private void HandleTurnStarted(UnitModel model)
    {
        var cells = _gameModel.GetAvailableMovePoints(model.Position.Value, model.ModifiedStats.MoveSpeed);
        ReachableCellsChanged.Invoke(cells);
    }

    private OperationResult Parse(string xRaw, string yRaw, out Vector2Int coords)
    {
        if (!int.TryParse(xRaw, out var x) || !int.TryParse(yRaw, out var y))
        {
            Error?.Invoke("Координаты должны быть числами");
            coords = default;
            return new OperationResult()
            {
                Message = ("Invalid coordinates"),
                IsSuccess = false
            };
        }
        coords = new Vector2Int(x, y);
        return new OperationResult(true);
    }

    public IGridCell GetCellContent(string text1, string text2)
    {
        Parse(text1, text2, out var coords);
        return _gameModel.GetCell(coords);
    }

    public bool IsPlayerTurn()
    {
        return true;
    }

    public void HandleCellSelected(Vector2Int coords)
    {
        Debug.Log("selectedCell" + selectedCell);
        selectedCell = coords;
        isCellSelected = true;
    }

    public void PerformAction(Vector2Int coords)
    {
        Debug.Log("performing action for " + coords + " from " + selectedCell);
        if (_gameModel.CurrentAction != null)
        {
            GameCell gameCell = (GameCell)_gameModel.GetCell(coords);
            _gameModel.PerformAction(gameCell);
        }
    }

    public void HandleCellHovered(Vector2Int coords)
    {
        if(_gameModel.CurrentAction != null)
        {
            GameCell gameCell = (GameCell)_gameModel.GetCell(coords);
            if (!_gameModel.CurrentAction.IsAvailable(gameCell))
                _cursorService.SetCursorState(CursorState.ActionNotAvailable);
            else
                _cursorService.SetCursorState(CursorState.ActionAvailable);
            var route = _gameModel.CurrentAction.GetRoute(gameCell);

            Debug.Log("HandleCellHovered" + route.Count);
            RoutePointsChanged?.Invoke(route);
        }
    }

    internal Vector2Int GetSelectedCell()
    {
        return selectedCell;
    }
}
