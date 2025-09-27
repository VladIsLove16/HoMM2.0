using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

public class GameViewModel : IDisposable
{
    public event Action<int, int> GridInitialized;
    public event Action<UnitViewModel> UnitStatsRequested;
    public event Action<PreviewResult> PreviewResultChanged;
    public event Action<DamageContextPreview> DamageContextPreviewChanged;
    public Action<UnitViewModel> UnitSpawned;

    private readonly GameModel _gameModel;
    private readonly MovementSystem _movementSystem;
    private readonly IGameCommandExecutor _gameCommandExecutor;
    private TurnSystem _turnSystem;
    private ActionResolver _actionResolver;
    private Dictionary<IGridContent, UnitViewModel> _uvms = new Dictionary<IGridContent, UnitViewModel>();
    private GameModel model;


    public GameViewModel(GameModel model, MovementSystem movementSystem, IGameCommandExecutor actionExecutor, TurnSystem turnSystem)
    {
        _gameModel = model;
        _movementSystem = movementSystem;
        _gameCommandExecutor = actionExecutor;
        _actionResolver = new(model,movementSystem);
        _turnSystem = turnSystem;

        _gameModel.GameChange_Initialized += OnGameModel_GridInitilized;
        _gameModel.GameChange_UnitSpawned += OnGameModel_UnitSpawned;
        _turnSystem.ActiveObject.Subscribe(_ => OnActiveUnitChanged());
    }

    public void HandleCellHovered(KeyValuePair<Vector2Int, Vector2Int> coords)
    {
        if (!_turnSystem.IsMyTurn)
            return;
        var fromCell = _turnSystem.ActiveObject.Value.Position;
        ActionContext actionContext = new(fromCell, coords.Key, default, coords.Value);
        _actionResolver.Resolve(actionContext, out var actionHandler);
        var previewResult =  actionHandler.GetPreview(actionContext);
        PreviewResult hoverPreviewResult =  new();
        hoverPreviewResult.Add(CellState.hovered, new List<Vector2Int>() { coords.Value });
        if (actionHandler is IAttackActionHandler attackActionHandler)
        {
            DamageContextPreviewChanged?.Invoke(attackActionHandler.GetDamagePreview(actionContext));
        }
        PreviewResultChanged?.Invoke(previewResult);
        PreviewResultChanged?.Invoke(previewResult);
    }

    public void HandleCellSelected(KeyValuePair<Vector2Int, Vector2Int> coords)
    {
        Debug.Log("HandleCellSelected");
        if (!_turnSystem.IsMyTurn)
            return;
        var fromCell = _turnSystem.ActiveObject.Value.Position;
        ActionContext actionContext = new(fromCell, coords.Key, default, coords.Value);
        _actionResolver.Resolve(actionContext, out var actionHandler);
        Debug.Log("resolved" + actionContext);

        _gameCommandExecutor.Execute(actionHandler.ActionType, actionContext);
    }

    public void HandleCellActionPerformed(KeyValuePair<Vector2Int, Vector2Int> coords)
    {
        var cell = _gameModel.GetCell(coords.Key);
        var unit = cell.Unit;
        if (_uvms.TryGetValue(unit,out var vm))
        {
            UnitStatsRequested?.Invoke(vm);
        }
    }

    protected virtual void OnGameModel_UnitSpawned(UnitModelCreatedParams @params)
    {
        UnitViewModel uvm = new(@params.UnitModel);
        _uvms[@params.UnitModel] = uvm;

        UnitSpawned?.Invoke(uvm);
    }

    public void Dispose()
    {
        _gameModel.GameChange_Initialized -= OnGameModel_GridInitilized;
        _gameModel.GameChange_UnitSpawned -= OnGameModel_UnitSpawned;
    }

    private void OnGameModel_GridInitilized(GridXZ<GameCell> g)
    {
        GridInitialized?.Invoke(g.GetWidth(), g.GetHeight());
    }

    private void OnActiveUnitChanged()
    {
        if (_turnSystem.IsMyTurn)
        {

        }
    }
    public bool CanExecute(ActionType type, ActionContext actionContext)
    {
        IActionHandler actionHandler = _actionResolver.Resolve(type, actionContext);
        return actionHandler.CanExecute(actionContext);
    }

    public void Execute(ActionType type, ActionContext actionContext)
    {
        IActionHandler actionHandler = _actionResolver.Resolve(type, actionContext);
        if (!actionHandler.CanExecute(actionContext))
            throw new Exception();
        actionHandler.Execute(actionContext);
    }

    public void SnapToCell(UnitViewModel draggedVM, Vector2Int coords)
    {
        var model = _uvms.First(UnitViewModel => UnitViewModel.Value == draggedVM).Key;
        _gameModel.MoveObject(model,coords);
    }
}
