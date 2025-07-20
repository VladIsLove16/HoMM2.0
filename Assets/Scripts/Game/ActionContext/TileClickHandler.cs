using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TileClickHandler : MonoBehaviour
{
    private ActionResolver _actionResolver;

    private UnitViewModel _selectedUnit;
    [Inject] private MovementSystem _movementSystem;
    [Inject] private GameModel _gameModel;
    public void OnTileClicked(Vector2Int cell)
    {
        if (_selectedUnit == null) return;

        var context = BuildActionContext(cell);
        _actionResolver.Resolve(context, out var handler);

        if (handler != null)
        {
            StartCoroutine(handler.Execute(context));
        }
    }

    public void OnTileHover(Vector2Int cell)
    {
        var context = BuildActionContext(cell);
        _actionResolver.Resolve(context,out var handler);

        if (handler != null)
            handler.ShowPreview(context);
    }

    private ActionContext BuildActionContext(Vector2Int cell)
    {
        IDamagable targetUnit = GetUnitAt(cell);
        //var isRanged = _selectedUnit.Model.ModifiedStats.AttackRange > 1;
        //List<Vector2Int> route;
        //if( targetUnit == null)
        //{
        //    _movementSystem.GetRoute(_selectedUnit.Model.Position.Value, cell, out route);
        //}
        //else
        //{
        //    _movementSystem.GetRoute(_selectedUnit.Model.Position.Value, cell, out var moveRoute);
        //    moveRoute.RemoveAt(moveRoute.Count - 1);
        //    route = moveRoute;
        //    //_movementSystem.GetPathToAttackPosition(_selectedUnit.Model, targetUnit);
        //}

        return new ActionContext
        {
            Unit = _selectedUnit,
            TargetCell = cell,
            TargetUnit = targetUnit,
            //IsRangedAttack = isRanged,
            //PlannedRoute = route
        };
    }

    private IDamagable GetUnitAt(Vector2Int pos)
    {
      return _gameModel.GetCell(pos).Unit;
    }

    public void SelectUnit(UnitViewModel vm)
    {
        _selectedUnit = vm;
        _actionResolver.SetHandlers(vm.GetAvailableAction());
    }
}
