using System.Collections.Generic;
using UnityEngine;

public class MovementSystem
{
    private GridXZ<GameCell> _grid;
    public MovementSystem(GridXZ<GameCell> grid)
    {
        _grid = grid;
    }
    public List<Vector2Int> GetReachableCells(Vector2Int startCell, int movementRange)
    {
        List<Vector2Int> reachableCells = new List<Vector2Int>();
        Dictionary<Vector2Int, float> moveCosts = new Dictionary<Vector2Int, float>();
        Queue<Vector2Int> cellsToCheck = new Queue<Vector2Int>();

        // Initialize with starting cell
        reachableCells.Add(startCell);
        moveCosts.Add(startCell, 0);
        cellsToCheck.Enqueue(startCell);

        while (cellsToCheck.Count > 0)
        {
            Vector2Int currentCell = cellsToCheck.Dequeue();
            float currentCost = moveCosts[currentCell];

            Vector2Int[] neighbors = new Vector2Int[]
            {
                currentCell + Vector2Int.up,
                currentCell + Vector2Int.right,
                currentCell + Vector2Int.down,
                currentCell + Vector2Int.left,
                currentCell + Vector2Int.up +  Vector2Int.right,
                currentCell + Vector2Int.up +  Vector2Int.left,
                currentCell + Vector2Int.down +  Vector2Int.right,
                currentCell + Vector2Int.down +  Vector2Int.left,
            };

            foreach (Vector2Int neighbor in neighbors)
            {
                if (!_grid.GetGridObject(neighbor.x,neighbor.y).IsEmpty)
                    continue;

                float newCost = currentCost + GetMovementCost(currentCell,neighbor);

                if (newCost <= movementRange &&
                   (!moveCosts.ContainsKey(neighbor) || newCost < moveCosts[neighbor]))
                {
                    if (!moveCosts.ContainsKey(neighbor))
                    {
                        reachableCells.Add(neighbor);
                    }

                    moveCosts[neighbor] = newCost;
                    cellsToCheck.Enqueue(neighbor);
                }
            }
        }

        return reachableCells;
    }

    private float GetMovementCost(Vector2Int fromCell, Vector2Int toCell)
    {
        return Mathf.Abs((fromCell-toCell).magnitude);
    }
}
