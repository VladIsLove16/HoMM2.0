using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameViewModel : IDisposable
{
    private readonly GameModel _model;
    private readonly MovementSystem _movementSystem;

    private Vector2Int? _selectedCell;

    public event Action<int, int> GridInitialized;
    public event Action<UnitViewModel> UnitSpawned;
    public event Action<UnitViewModel> UnitRemoved;
    private Dictionary<UnitModel, UnitViewModel> _uvms;
    [Inject] private UnitViewModelFactory _unitViewModelsFactory;
    public GameViewModel(GameModel model, MovementSystem movementSystem)
    {
        _model = model;
        _movementSystem = movementSystem;

        _model.UnitSpawned += OnUnitSpawned;
        _model.UnitRemoved += OnUnitRemoved;
        //Model.UnitMovedByRoute += (unit, path) => UnitMovedByRoute?.Invoke(unit, path);
        _model.GridInitialized += g => GridInitialized?.Invoke(g.GetWidth(), g.GetHeight());
    }

    private void OnUnitRemoved(UnitModelRemovedParams @params)
    {
        UnitViewModel uvm = _uvms[@params.UnitModel];
        UnitRemoved?.Invoke(uvm);
    }

    protected virtual void OnUnitSpawned(UnitModelCreatedParams @params)
    {
        UnitViewModel uvm = _unitViewModelsFactory.Create(@params.UnitModel);
        UnitSpawned?.Invoke(uvm);
    }

    public void SpawnRandomUnit()
    {
        var (x, y) = _model.GetRandomEmpty();
        _model.SpawnUnit(new UnitSpawnParams(x, y));
    }

    // Управление выбранной клеткой
    public void SetSelectedCell(Vector2Int cell)
    {
        _selectedCell = cell;
    }

    public Vector2Int? GetSelectedCell()
    {
        return _selectedCell;
    }

    // Метод для создания юнитов на основе данных (делегируем модели)
    public void CreateGridContent(GridContentEntrySO unitContentEntrySO)
    {
        _model.ClearGrid();
        foreach (var content in unitContentEntrySO.contents)
        {
            UnitSpawnParams unitSpawnParams = new UnitSpawnParams(
                content.X,
                content.Y,
                content.unitType,
                content.Amount,
                content.isPlayer
            );
            _model.SpawnUnit(unitSpawnParams);
        }
    }

    public void Dispose()
    {
        // Отписка от событий модели
        _model.UnitSpawned -= OnUnitSpawned;
        _model.UnitRemoved -= OnUnitRemoved;
        //Model.UnitMovedByRoute -= (unit, path) => UnitMovedByRoute?.Invoke(unit, path);
    }
}
public class GameViewModelDebugger : GameViewModel
{
    GameViewModelDebugger(GameModel model, MovementSystem movementSystem) : base(model, movementSystem) {
        Debug.Log("GameViewModel is ready");
    }
    protected override void OnUnitSpawned(UnitModelCreatedParams @params)
    {
        Debug.Log("GameViewModel OnUnitSpawned called " + @params.UnitModel.ToString());
        base.OnUnitSpawned(@params);
    }
}