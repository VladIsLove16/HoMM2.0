using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class GameGridViewModel : IInitializable
{
    private GameGridModel _model;
    private readonly UnitFactory _unitFactory;
    public Action<string> OnError;
    internal Action< string> OnSelectedCellChanged;
    public event Action<GridCellChangedEventArgs> OnCellChanged;
    [Inject]
    public GameGridViewModel(
        GameGridModel model,
        UnitFactory unitFactory
    )
    {
        _model = model;
        _unitFactory = unitFactory;
    }
    public List<GameGridCell> GetCells()
    {
        return _model.GetCellModels();
    }

    internal GameGridCell GetCell(string xStr, string yStr)
    {
        if (!int.TryParse(xStr, out var x) || !int.TryParse(yStr, out var y))
        { OnError?.Invoke("Неверные координаты"); return null; }
        return _model.GetCell(x, y);
    }
    public bool TryPlaceUnit(int x, int y, UnitModel unit)
    {
        if (!CanSpawnAt(x, y))
            return false;
        _model.TryAddContent(x, y, unit);
        return true;
    }
    
    public void SelectCell(string xRaw, string yRaw)
    { 

    }

    public int GetWidth()
    {
        return _model.GetWidth();
    }
    public int GetHeight()
    {
        return _model.GetHeight();
    }
    public void Initialize()
    {
        _model.OnGridObjectChanged += HandleGridObjectChanged;
    }  
    public void SpawnRandomUnit()
    {
        _model.GetCellModels().First(x => x.IsEmpty());
    }

    public void SpawnUnitAt(string xRaw, string yRaw)
    {
        if (!int.TryParse(xRaw, out var x) || !int.TryParse(yRaw, out var y))
        { OnError?.Invoke("Неверные координаты"); return; }

        var result = SpawnUnitAt(x, y, UnitType.Archer, 3);
        if (!result.IsSuccess)
            OnError?.Invoke(result.ErrorMessage);
    }

    public OperationResult SpawnUnitAt(int x, int y, UnitType unitType, int amount)
    {
        var (vm, view) = _unitFactory.Create(unitType, amount, x,y);

        if (!TryPlaceUnit(x, y, vm.Model))
        {
            OnError?.Invoke($"Не удалось заспавнить {unitType} в ({x},{y}).");
            GameObject.Destroy(view.gameObject);
            return new OperationResult() { IsSuccess = false };
        }
        else
            return new OperationResult() { IsSuccess = true };
    }

    public bool CanSpawnAt(int x, int y)
    {
        return _model.IsEmpty(x, y);
    }

    private void HandleGridObjectChanged(object sender, GridCellChangedEventArgs args)
    {
        string desc = _model.GetCell(args.x, args.y).GetDescription();
        OnCellChanged?.Invoke(args);
    }

    internal string GetDescription(int x, int y)
    {
        return _model.GetCell(x, y).GetDescription();
    }

}
