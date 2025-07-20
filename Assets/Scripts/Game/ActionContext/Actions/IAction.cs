using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
public interface IAction
{
    bool IsAvailable(GameCell gameCell);
    bool Perform(GameCell gameCell);
    void AddTarget(GameCell gameCell);
    List<(Vector2Int,bool)> GetRoute(GameCell gameCell);
}