using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveUnitAcion : IAction
{
    private List<GameCell> _cells = new List<GameCell>();
    private GameCell fromCell => _cells[0];
    private MovementSystem _movementSystem;
    private List<Vector2Int> route;
    private event Action<List<Vector2Int>> performed;
    private int moveSpeed;
    private List<(Vector2Int, bool)> lastCachedRoute = new();
    public MoveUnitAcion(MovementSystem movementSystem, GameCell fromCell,int moveSpeed, Action<List<Vector2Int>> performed)
    {
        _movementSystem = movementSystem;
        AddTarget(fromCell);
        this.performed = performed;
        this.moveSpeed = moveSpeed;
    }
    public void AddTarget(GameCell gameCell)
    {
        _cells.Add(gameCell);
    }

    public bool IsAvailable(GameCell gameCell)
    {
        if(_movementSystem.GetRoute(fromCell.Position, gameCell.Position, out route))
        {
            lastCachedRoute.Clear();
            List<Vector2Int> accesibles = _movementSystem.GetAccessibleRoutePoints(route,moveSpeed);
            Debug.Log(" accessibles " + _movementSystem.RouteToString(accesibles));
            List<Vector2Int> notAccesibles = route.Where(point => !accesibles.Contains(point)).ToList();
            foreach(var point in accesibles)
            {
                lastCachedRoute.Add((point,true));
            }
            foreach(var point in notAccesibles)
            {
                lastCachedRoute.Add((point,false));
            }
            float routeCost = _movementSystem.GetRouteCost(route);
            return Mathf.Approximately(routeCost, moveSpeed) || routeCost<moveSpeed;
        }
        else 
            return false;
    }

    public bool Perform(GameCell gameCell)
    {
        if(!IsAvailable(gameCell))
        {
            return false;
        }
        GameCell fromCell = this.fromCell;
        GameCell toCell = gameCell;
        UnitModel fromUnit = fromCell.GetUnit();
        if (fromUnit == null)
        {
            Debug.LogWarning("no unit in " + fromCell.x + " " + fromCell.y);
            return false;
        }
        if(!_movementSystem.GetRoute(fromCell.Position,toCell.Position, out var route))
        {
            throw new InvalidOperationException("route not found but action is available? thats cant be! Check ur code bro");
        }
        fromCell.RemoveUnit();
        toCell.AddContent(fromUnit);
        performed?.Invoke(route);
        return true;
    }

    public List<(Vector2Int,bool)> GetRoute(GameCell gameCell)
    {
       return lastCachedRoute ?? new List<(Vector2Int,bool)>() { (gameCell.Position, true )};
    }
}