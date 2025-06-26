using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// ViewModel: связывает GameModel и представления, обрабатывает команды и генерирует готовые к отображению данные.
/// </summary>
public class GameViewModel : IInitializable, IDisposable
{
    private readonly GameModel _gameModel;

    public event Action<string> OnError;
    public event Action<string> OnLastChangedCellDescription;
    //public event Action<string> OnSelectedCellDescription;

    public Action<UnitModel> OnCellContentAdded;
    public Action<UnitModel> OnCellContentRemoved;
    public Action<UnitModel, UnitModel> OnCellContentSwaped;
    public Action<UnitModel, Vector2Int> OnCellContentMoved;
    public Action<List<Vector2Int>> OnReachableCellsChanged;
    public Action<UnitModel> OnTurnStarted;

    private Vector2Int selectedCell;
    private bool isCellSelected;
    public GameViewModel(GameModel model)
    {
        _gameModel = model;
        Initialize();
    }

    public void Initialize()
    {
        _gameModel.OnCellContentAdded += HandleContentAdded;
        _gameModel.OnCellContentRemoved += HandleContentRemoved;
        _gameModel.OnCellContentSwaped += HandleContentSwaped;
        _gameModel.OnCellContentMoved += HandleContentMoved;
        _gameModel.OnTurnStarted += HandleTurnStarted;
    }

    public void Dispose()
    {
        _gameModel.OnCellContentAdded -= HandleContentAdded;
        _gameModel.OnCellContentRemoved -= HandleContentRemoved;
        _gameModel.OnCellContentSwaped -= HandleContentSwaped;
        _gameModel.OnCellContentMoved -= HandleContentMoved;
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
            OnError?.Invoke("Координаты должны быть числами");
            return new OperationResult(false, "Invalid coordinates");
        }
        UnitSpawnParams @params = new(x, y, type);
        var result = _gameModel.SpawnUnit(@params);
        if (!result.IsSuccess)
            OnError?.Invoke(result.Message);
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
            OnError?.Invoke(result.Message);
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
    //        OnError?.Invoke(ex.Message);
    //        return new OperationResult()
    //        {
    //            Message = ex.Message,
    //            IsSuccess = false
    //        };
    //    }
    //}

    private void HandleContentAdded(UnitModelCreatedParams e)
    {
        OnCellContentAdded?.Invoke(e.UnitModel);
        var desc = _gameModel.GetCell(e.UnitModel.X, e.UnitModel.Y).ToString();
        OnLastChangedCellDescription?.Invoke(desc);
    }

    private void HandleContentRemoved(UnitModelRemovedParams e)
    {
        Debug.Log("content removed");
        OnCellContentRemoved?.Invoke(e.UnitModel);
    }
    private void HandleContentSwaped(UnitModelsSwapped e)
    {
        Debug.Log(" HandleContentSwaped(UnitModelsSwapped swapped)");
        OnCellContentSwaped?.Invoke(e.from, e.to);
    }
    private void HandleContentMoved(UnitModel model, Vector2Int to)
    {
        Debug.Log(" OnCellContentMoved?.Invoke(viewModel, to);");
        OnCellContentMoved?.Invoke(model, to);
    }

    private void HandleTurnStarted(UnitModel model)
    {
        var cells = _gameModel.GetAvailableMovePoints(model.Coodrs, model.UnitStats.Speed);
        OnReachableCellsChanged.Invoke(cells);
    }


    private OperationResult Parse(string xRaw, string yRaw, out Vector2Int coords)
    {
        if (!int.TryParse(xRaw, out var x) || !int.TryParse(yRaw, out var y))
        {
            OnError?.Invoke("Координаты должны быть числами");
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

    public void HandleActionPerformed(Vector2Int coords)
    {
        Debug.Log("performing action for " + coords + " from " + selectedCell);
        if (isCellSelected && _gameModel.SwapUnits(coords, selectedCell))
        {
            isCellSelected = false;
        }
        else if (isCellSelected && _gameModel.MoveUnit(coords, selectedCell))
        {
            selectedCell = coords;
            isCellSelected = true;
        }
    }

    public void HandleCellHovered(Vector2Int coords)
    {

    }

    internal Vector2Int GetSelectedCell()
    {
        return selectedCell;
    }
}
