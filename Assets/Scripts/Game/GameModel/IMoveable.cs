// GameModel.cs
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveable : IGridContent
{
    void MoveByRoute(List<Vector2Int> route);
}