using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionService
{
    public IAction CurrentAction { get; private set; }

    public event Action<List<Vector2Int>> RouteChanged;

    public void SetAction(IAction action)
    {
        CurrentAction = action;

        //if (action is MoveUnitAcion moveAction)
        //{
        //    moveAction.OnRouteChanged = (route) => RouteChanged?.Invoke(route);
        //}
    }

    public void CancelAction()
    {
        CurrentAction = null;
    }

    public bool TryPerformAction(GameCell targetCell, TurnSystem turnSystem)
    {
        if (CurrentAction == null)
            return false;

        bool success = CurrentAction.Perform(targetCell);
        if (success)
        {
            turnSystem.EndTurn();
            CancelAction();
        }

        return success;
    }

    public void ShowPreview(GameCell targetCell)
    {
        if (CurrentAction is IPreviewable previewable)
        {
            previewable.ShowPreview(targetCell);
        }
    }
}
