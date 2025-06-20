using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Zenject;

/// <summary>
/// ViewModel: связывает GameGridModel и представления, обрабатывает команды и генерирует готовые к отображению данные.
/// </summary>
public class GameGridViewModel : IInitializable, IDisposable
{
    private readonly GameGridModel _model;

    public event Action<string> OnError;
    public event Action<string> OnLastChangedCellDescription;
    public event Action<string> OnSelectedCellDescription;

    public Action<UnitViewModel> OnCellContentAdded;
    public Action<UnitViewModel> OnCellContentRemoved;

    private Dictionary<UnitModel, UnitViewModel> _models = new();
    public GameGridViewModel(GameGridModel model)
    {
        _model = model;
        Initialize();
    }

    public void Initialize()
    {
        _model.OnCellContentAdded += HandleContentAdded;
        _model.OnCellContentRemoved += HandleContentRemoved;
    }

    public void Dispose()
    {
        _model.OnCellContentAdded -= HandleContentAdded;
        _model.OnCellContentRemoved -= HandleContentRemoved;
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
    //        var cell = _model.GetCell(x, y);
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
}
