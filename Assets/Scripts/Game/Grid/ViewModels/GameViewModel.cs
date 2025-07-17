using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// ViewModel: связывает GameModel и представления, обрабатывает команды и генерирует готовые к отображению данные.
/// </summary>
public class GameViewModel : IInitializable, IDisposable
{
    private readonly GameModel _model;

    public event Action<string> OnError;
    public event Action<string> OnLastChangedCellDescription;
    public event Action<string> OnSelectedCellDescription;

    public Action<UnitViewModel> OnCellContentAdded;
    public Action<UnitViewModel> OnCellContentRemoved;
    public Action<UnitViewModel, UnitViewModel> OnCellContentSwaped;
    public Action<UnitViewModel, Vector2Int> OnCellContentMoved;
    public Action<UnitViewModel> OnTurnStarted;

    private Dictionary<UnitModelLegacy, UnitViewModel> _models = new();

    public GameViewModel(GameModel model)
    {
        _model = model;
        Initialize();
    }

    public void Initialize()
    {
        _model.OnCellContentAdded += HandleContentAdded;
        _model.OnCellContentRemoved += HandleContentRemoved;
        _model.OnCellContentSwaped += HandleContentSwaped;
        _model.OnCellContentMoved += HandleContentMoved;
        _model.OnTurnStarted += HandleTurnStarted;
    }


    public void Dispose()
    {
        _model.OnCellContentAdded -= HandleContentAdded;
        _model.OnCellContentRemoved -= HandleContentRemoved;
        _model.OnCellContentSwaped -= HandleContentSwaped;
        _model.OnCellContentMoved -= HandleContentMoved;
    }

    public int GetWidth() => _model.GetWidth();
    public int GetHeight() => _model.GetHeight();
    public IReadOnlyList<IGridCell> GetCells() => _model.GetAllCells();

    /// <summary>
    /// Спавнит юнит, парсит координаты и вызывает модель.
    /// </summary>
    public OperationResult SpawnUnitAt(string xRaw, string yRaw, UnitType type = 0)
    {
        if (!int.TryParse(xRaw, out var x) || !int.TryParse(yRaw, out var y))
        {
            OnError?.Invoke("Координаты должны быть числами");
            return new OperationResult(false,"Invalid coordinates");
        }
        UnitSpawnParams @params = new(x, y, type);
        var result = _model.SpawnUnit(@params);
        if (!result.IsSuccess)
            OnError?.Invoke(result.Message);
        return result;
    }

    /// <summary>
    /// Спавнит юнит в случайной пустой клетке.
    /// </summary>
    public void SpawnUnitAtRandomPlace()
    {
        var (x, y) = _model.GetEmpty();
        UnitSpawnParams @params = new(x, y);
        var result = _model.SpawnUnit(@params);
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
        UnitViewModel unitViewModel = new UnitViewModel(e.UnitModel);
        if(_models == null)
        {
            _models = new();
        }
        _models[e.UnitModel] = unitViewModel;

        Debug.Log($"unitViewModel {unitViewModel.UnitType} created {unitViewModel.X} {unitViewModel.Y}");
        OnCellContentAdded?.Invoke(unitViewModel);
        var desc = _model.GetCell(e.UnitModel.X, e.UnitModel.Y).ToString();
        OnLastChangedCellDescription?.Invoke(desc);
    }

    private void HandleContentRemoved(UnitModelRemovedParams e)
    {
        UnitViewModel unitViewModel = _models[e.UnitModel];
        _models.Remove(e.UnitModel);
        Debug.Log("content removed");
        OnCellContentRemoved?.Invoke(unitViewModel);
    }
    private void HandleContentSwaped(UnitModelsSwapped swapped)
    {
        UnitViewModel toViewModel = _models[swapped.to];
        UnitViewModel fromViewModel = _models[swapped.from];
        Debug.Log(" HandleContentSwaped(UnitModelsSwapped swapped)");
        OnCellContentSwaped?.Invoke(toViewModel, fromViewModel);
    }
    private void HandleContentMoved(UnitModelLegacy model, Vector2Int to)
    {
        UnitViewModel viewModel = _models[model];
        Debug.Log(" OnCellContentMoved?.Invoke(viewModel, to);");
        OnCellContentMoved?.Invoke(viewModel, to);
    }

    private void HandleTurnStarted(UnitModelLegacy model)
    {
        OnTurnStarted?.Invoke(_models[model]);
    }

    private OperationResult Parse( string xRaw, string yRaw, out Vector2Int coords)
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
        coords = new Vector2Int(x,y);
        return new OperationResult(true);
    }

    internal IGridCell GetCellContent(string text1, string text2)
    {
        Parse(text1, text2, out var coords);
        return _model.GetCell(coords);
    }

    internal bool IsPlayerTurn()
    {
        throw new NotImplementedException();
    }

    internal void GetAvailableMoves()
    {
        throw new NotImplementedException();
    }
}
