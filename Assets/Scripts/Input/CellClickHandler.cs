using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

public partial class PlayerInputHandler
{
    [Inject] private ActionResolver _actionResolver;
    [Inject] private GameModel _gameModel;
    [Inject] private GameInputHandler3D _gameInputHandler;
    [Inject] private UnitStatsPanel _unitStatsPanel;
    [Inject] private TurnSystem _turnSystem;
    public ReactiveProperty<IActionHandler> CurrentAction = new();
    public PlayerInputHandler(GameInputHandler3D gameInputHandler3D, UnitStatsPanel unitStatsPanel, GameModel gameModel, TurnSystem turnSystem)
    {
        _gameInputHandler = gameInputHandler3D;
        _unitStatsPanel = unitStatsPanel;
        _gameModel = gameModel;
        _turnSystem = turnSystem;

        _gameInputHandler.HoveredCell.Skip(1).Subscribe(OnCellHovered);
        _gameInputHandler.SelectedCell.Skip(1).Subscribe(OnCellSelected);
        _gameInputHandler.ActionPerformed.Skip(1).Subscribe(OnActionPerformed);

        turnSystem.ActiveObject.Skip(1).Subscribe(OnActiveObjectChanged);

    }

    private void OnActiveObjectChanged(ICombatObject @object)
    {
        if (@object is UnitModel unit)
        {
            _actionResolver.SetActions(unit);

            foreach (var h in _actionResolver._handlers)
            {
                h.ShowAvaiableTargetCells();
            }
        }
    }


    protected virtual void OnCellHovered(Vector2Int cell)
    {
        var context = BuildActionContext(cell);
        _actionResolver.Resolve(context, out var handler);

        if (handler != null)
        {
            CurrentAction.SetValueAndForceNotify(handler);
            handler.ShowPreview(context);
        }
    }

    protected virtual void OnCellSelected(Vector2Int cell)
    {
        var context = BuildActionContext(cell);
        _actionResolver.Resolve(context, out var handler);

        if (handler != null)
        {
            handler.Execute(context);
        }
    }
    protected virtual void OnActionPerformed(Vector2Int cell)
    {
        ShowUnitStatsPanel(cell);
    }
    protected virtual void ShowUnitStatsPanel(Vector2Int cell)
    {
        if(!GetUnitAt(cell, out var model))
        {
            return;
        }
        UnitStatsViewModel unitStatsViewModel = new(model);
        _unitStatsPanel.Init(unitStatsViewModel);
        _unitStatsPanel.Show();
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
public class CellClickHandlerDebugger : PlayerInputHandler
{
    public CellClickHandlerDebugger(GameInputHandler3D gameInputHandler3D, UnitStatsPanel unitStatsPanel, GameModel gameModel,TurnSystem turnSystem) : base(gameInputHandler3D, unitStatsPanel,gameModel, turnSystem)
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
        Debug.Log("CellClickHandlerDebugger GetUnitAt called " + pos);
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