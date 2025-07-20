using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//public class SwapUnitsAction : IAction
//{
//    private List<GameCell> targets;
//    public void AddTarget(GameCell gameCell)
//    {
//        targets.Add(gameCell);
//    }

//    public bool IsAvailable(GameCell gameCell)
//    {
//        if (targets.Count != 1)
//        {
//            Debug.Log("SwapUnitsAction is not available. No target unit ");
//            return false;
//        }
//        if (!targets[0].ContainsUnit())
//        {
//            Debug.Log("SwapUnitsAction is not available. Target must contains unit");
//            return false;
//        }
//        if (!gameCell.ContainsUnit())
//        {
//            Debug.Log("TeleportUnitAction is not available. GameCells[1] must contains unit");
//            return false;
//        }
//        return true;
//    }

//    public bool Perform()
//    {
//        if (!IsAvailable()) {
//            return false;
//        }
//        GameCell fromCell = targets[0];
//        GameCell toCell = targets[1];
//        UnitModel fromUnit = fromCell.GetUnit();
//        UnitModel toUnit = toCell.GetUnit();
//        if (fromUnit == null || toUnit == null)
//        {
//            Debug.Log("SwapUnitsAction is not available. GameCells must contain units");
//            return false;
//        }
//        fromCell.RemoveUnit();
//        toCell.RemoveUnit();
//        fromCell.AddContent(toUnit);
//        toCell.AddContent(fromUnit);
//        return true;
//    }
//}
public class TeleportUnitAction : IAction
{
    private List<GameCell> targets;
    private Action<Vector2Int> OnPerformed;
    public TeleportUnitAction(GameCell from,Action<Vector2Int> OnPerformed)
    {
        targets = new();
        targets.Add(from);
        this.OnPerformed += OnPerformed;
    }
    public void AddTarget(GameCell gameCell)
    {
        targets.Add(gameCell);
    }

    public bool IsAvailable(GameCell gameCell)
    {
        if (targets.Count != 1)
        {
            return false;
        }
        if (!targets[0].ContainsUnit())
        {
            return false;
        }
        if (!gameCell.IsEmpty)
        {
            return false;
        }
        return true;
    }

    public bool Perform(GameCell gameCell)
    {
        if (!IsAvailable(gameCell)) 
            return false;

        GameCell fromCell = targets[0];
        GameCell toCell = gameCell;
        UnitModel fromUnit = fromCell.GetUnit();
        if (fromUnit == null)
        {
            Debug.LogWarning("no unit in " + fromCell.x + " " + fromCell.y);
            return false;
        }

        fromCell.RemoveUnit();
        toCell.AddContent(fromUnit);
        OnPerformed?.Invoke(new Vector2Int(toCell.X,toCell.y));
        return true;
    }

    List<(Vector2Int, bool)> IAction.GetRoute(GameCell gameCell)
    {
       return new() { (targets[0].Position, true),(gameCell.Position, IsAvailable(gameCell)) };
    }
}