using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Zenject;

public partial class PlayerInputHandler
{
    [Inject] protected MoveThenAttackHandlerFactory _moveThenAttackFactory = new();
    [Inject] protected MoveActionHandlerFactory _moveActionFactory = new();
    [Inject] protected RangedAttackHandlerFactory _rangedAttackHandlerFactory = new();
    MoveActionHandler _moveActionHandler;
    MoveThenAttackHandler _moveThenAttackHandler;
    RangedAttackHandler _rangedAttackHandler;
    //[Inject] private ActionResolver _actionResolver;
    [Inject] private GameModel _gameModel;
    [Inject] private GameInputHandler3D _gameInputHandler;
    [Inject] private UnitStatsPanel _unitStatsPanel;
    [Inject] private TurnSystem _turnSystem;
    [Inject] private MovementSystem _movementSystem;
    IActionHandler currentACtionView;
    ICombatObject activeObject;
    public PlayerInputHandler(GameInputHandler3D gameInputHandler3D, UnitStatsPanel unitStatsPanel, GameModel gameModel, TurnSystem turnSystem)
    {
        _gameInputHandler = gameInputHandler3D;
        _unitStatsPanel = unitStatsPanel;
        _gameModel = gameModel;
        _turnSystem = turnSystem;

        _gameInputHandler.HoveredCell.Skip(1).Subscribe(OnCellHovered);
        _gameInputHandler.SelectedCell.Skip(1).Subscribe(OnCellSelected);
        _gameInputHandler.ActionPerformed.Skip(1).Subscribe(OnActionPerformed);
        _gameInputHandler.ActionCanceled+=OnActionCanceled;

        turnSystem.ActiveObject.Skip(1).Subscribe(OnActiveObjectChanged);
    }

    private void OnActionCanceled()
    {
        //if (CurrentAction != null)
        //{
        //    CurrentAction.Cancel();
        //}
    }

    private void OnActiveObjectChanged(ICombatObject @object)
    {
        Debug.Log("OnActiveObjectChanged " + @object.ToString());
        if (@object is UnitModel unit)
        {
            _moveActionHandler = _moveActionFactory.Create(unit);
            _rangedAttackHandler = _rangedAttackHandlerFactory.Create(unit);
            _moveThenAttackHandler = _moveThenAttackFactory.Create(unit);
            _moveActionHandler.ShowAvaiableTargetCells();
        }
        activeObject = @object;
    }


    protected virtual void OnCellHovered(Vector2Int cell)
    {
        var context = BuildActionContext(cell);
        if (context .TargetObject != null)
        {
            //if (_movementSystem.HasLineOfSight(activeObject.Position, cell))
            //{
            currentACtionView?.HidePreview();
            _rangedAttackHandler.ShowPreview(context);
            currentACtionView = _rangedAttackHandler;
            //}
        }
        else
        {
            currentACtionView?.HidePreview();
            _moveActionHandler.ShowPreview(context);
            currentACtionView = _moveActionHandler;
            
        }
    }
    protected virtual void OnCellSelected(Vector2Int cell)
    {
        var context = BuildActionContext(cell);
        if (context.TargetObject != null)
        {
            //if (_movementSystem.HasLineOfSight(activeObject.Position, cell))
            //{
            _rangedAttackHandler.Execute(context);
            _turnSystem.EndTurn();
            //}
        }
        else
        {
            _moveActionHandler.Execute(context);
            _turnSystem.EndTurn();
        }
            
    }
    protected virtual void OnActionPerformed(Vector2Int cell)
    {
        ShowUnitStatsPanel(cell);
    }
    protected virtual void ShowUnitStatsPanel(Vector2Int cell)
    {
        if (!GetUnitAt(cell, out var model)) return;

        var vm = new UnitStatsViewModel(model);
        _unitStatsPanel.Init(vm); // Подписка на изменения
    }

    private ActionContext BuildActionContext(Vector2Int cell)
    {
        IDamagable target = GetTargetAt(cell);

        return new ActionContext
        {
            TargetCell = cell,
            TargetObject = target,
            //IsRangedAttack = isRanged,
            //PlannedRoute = route
        };
    }

    protected virtual IDamagable GetTargetAt(Vector2Int pos)
    {
      return _gameModel.GetCell(pos).Unit;
    }

    protected virtual bool GetUnitAt(Vector2Int pos, out UnitModel unitModel)
    {
        unitModel = _gameModel.GetCell(pos).Unit;
        if (unitModel==null)
            return false;
        return true;
    }

}
public class PlayerInputHandlerDebugger : PlayerInputHandler
{
    public PlayerInputHandlerDebugger(GameInputHandler3D gameInputHandler3D, UnitStatsPanel unitStatsPanel, GameModel gameModel,TurnSystem turnSystem) : base(gameInputHandler3D, unitStatsPanel,gameModel, turnSystem)
    {
        Debug.Log("PlayerInputHandler is ready");
    }
    protected override void OnActionPerformed(Vector2Int cell)
    {
        Debug.Log("OnActionPerformed " + cell);
        base.OnActionPerformed(cell);
    }
    protected override void OnCellSelected(Vector2Int cell)
    {
        Debug.Log("OnCellSelected " + cell);
        base.OnCellSelected(cell);
    }
    protected override void OnCellHovered(Vector2Int cell)
    {
        Debug.Log("OnCellHovered " + cell);
        base.OnCellHovered(cell);
    }
    protected override bool GetUnitAt(Vector2Int pos, out UnitModel unitModel)
    {
        Debug.Log("PlayerInputHandlerDebugger GetUnitAt called " + pos);
        bool res = base.GetUnitAt(pos, out unitModel);
        if (!res)
            Debug.Log("unitModel is null");
        else
            Debug.Log("unitModel is " + unitModel.UnitType);
            return res;
    }
    protected override void ShowUnitStatsPanel(Vector2Int cell)
    {
        base.ShowUnitStatsPanel(cell);
    }
}