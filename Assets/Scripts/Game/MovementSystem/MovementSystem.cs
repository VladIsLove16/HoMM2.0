using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementSystem
{
    private GridXZ<GameCell> _grid;
    private Dictionary<Vector2Int, List<Vector2Int>> reachableCellsCache = new();
    private Dictionary<(Vector2Int,Vector2Int), List<Vector2Int>> routesCache = new();
    private Dictionary<(Vector2Int,Vector2Int), List<Vector2Int>> ignoreObstaclesRoutesCache = new();
    public void Init(GridXZ<GameCell> grid)
    {
        _grid = grid;
        _grid.GridObjectChanged += OnGridChanged;
    }
    private void OnGridChanged(GameCell gameCell)
    {
        ClearCaches();
    }

    private void ClearCaches()
    {
        reachableCellsCache.Clear();
        routesCache.Clear();
        ignoreObstaclesRoutesCache.Clear();
    }

    public List<Vector2Int> GetReachableCells(Vector2Int startCell, int movementRange)
    {
        if (reachableCellsCache.TryGetValue(startCell, out var cached))
            return cached;

        bool _ = RunPathfinding(startCell, movementRange, null, out List<Vector2Int> reachableCells, out var route);
        reachableCellsCache[startCell] = reachableCells;
        return reachableCells;
    }

    public bool GetRoute(Vector2Int fromCell, Vector2Int toCell, out List<Vector2Int> route)
    {
        if (routesCache.TryGetValue((fromCell, toCell), out route))
        {
            return true;
        }
        if(RunPathfinding(fromCell, int.MaxValue, toCell, out _, out route))
        {
            return true;
        }
        return false;
    }

    public bool GetRouteIgnoringObstacles(Vector2Int fromCell, Vector2Int toCell, out List<Vector2Int> route)
    {
        if (ignoreObstaclesRoutesCache.TryGetValue((fromCell, toCell), out route))
        {
            Debug.Log("route found in cache " + RouteToString(route));
            return true;
        }
        if (RunPathfinding(fromCell, int.MaxValue, toCell, out _, out route,true))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Объединённый алгоритм для получения достижимых клеток и маршрута.
    /// </summary>
    private bool RunPathfinding(
        Vector2Int startCell,
        int movementRange,
        Vector2Int? targetCell,
        out List<Vector2Int> reachableCells,
        out List<Vector2Int> route,
        bool ignoreOstacles = false)
    {
        reachableCells = new List<Vector2Int>();
        route = new List<Vector2Int>();

        Dictionary<Vector2Int, float> moveCosts = new();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Queue<Vector2Int> open = new();

        moveCosts[startCell] = 0;
        open.Enqueue(startCell);
        reachableCells.Add(startCell);

        while (open.Count > 0)
        {
            Vector2Int current = open.Dequeue();
            float costSoFar = moveCosts[current];

            // Check for goal
            if (targetCell.HasValue && current == targetCell.Value)
            {
                // Build path
                Vector2Int step = current;
                while (step != startCell)
                {
                    route.Add(step);
                    step = cameFrom[step];
                }
                route.Add(startCell);
                route.Reverse();
                var target = (Vector2Int)targetCell;
                routesCache[(startCell, target)] = route;
                return true;
            }

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!_grid.TryGetGridObject(neighbor.x, neighbor.y, out var cell))
                    continue;
                if(!cell.IsEmpty && !ignoreOstacles)
                    continue;

                float newCost = costSoFar + GetDistanceMagnitude(current, neighbor);

                if (newCost <= movementRange && (!moveCosts.ContainsKey(neighbor) || newCost < moveCosts[neighbor]))
                {
                    moveCosts[neighbor] = newCost;
                    cameFrom[neighbor] = current;
                    open.Enqueue(neighbor);

                    if (!reachableCells.Contains(neighbor) && cell.IsEmpty)
                    {
                        reachableCells.Add(neighbor);
                    }
                }
            }
        }
        return targetCell == null;
    }

    private float GetDistanceMagnitude(Vector2Int from, Vector2Int to)
    {
        return (from - to).magnitude;
    }

    public float GetRouteCost(List<Vector2Int> route)
    {
        var tempRoute = route.ToList();
        float cost = 0;
        Vector2Int start = tempRoute[0];
        tempRoute.Remove(start);
        foreach (var a in route)
        {
            cost += GetDistanceMagnitude(start, a);
            start = a;
        }
        return cost;
    }

    public List<Vector2Int> GetAccessibleRoutePoints(List<Vector2Int> route,int moveSpeed)
    {
        var tempRoute = route.ToList();
        var resultRoute = new List<Vector2Int>();
        float cost = 0;
        Vector2Int start = tempRoute[0];
        tempRoute.Remove(start);
        resultRoute.Add(start);
        foreach (var a in tempRoute)
        {
            cost += GetDistanceMagnitude(start, a);
            start = a;
            if(cost<=moveSpeed)
            {
                resultRoute.Add(a);
            }
            else
                return resultRoute;
        }
        return resultRoute;
    }
    public bool HasLineOfSight(Vector2Int from, Vector2Int to)
    {
        var line = GetLine(from, to);

        foreach (var pos in line)
        {
            // Пропускаем начальную и конечную точки
            if (pos == from || pos == to)
                continue;

            if (!_grid.TryGetGridObject(pos.x, pos.y, out var cell))
                return false;

            foreach (var content in cell.Contents)
            {
                if (content is IBlocksLineOfSight blocker && blocker.BlocksSight)
                    return false;
            }
        }

        return true;
    }
    private List<Vector2Int> GetLine(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> line = new();
        int x0 = start.x;
        int y0 = start.y;
        int x1 = end.x;
        int y1 = end.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);

        int sx = (x0 < x1) ? 1 : -1;
        int sy = (y0 < y1) ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            line.Add(new Vector2Int(x0, y0));

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return line;
    }

    public string RouteToString(List<Vector2Int> vector2Ints)
    {
        string path = "path: ";
        foreach(var  a in vector2Ints)
        {
            path += a.ToString();
        }
        return path;
    }
    private Vector2Int[] GetNeighbors(Vector2Int cell)
    {
        return new Vector2Int[]
        {
            cell + Vector2Int.up,
            cell + Vector2Int.right,
            cell + Vector2Int.down,
            cell + Vector2Int.left,
            cell + Vector2Int.up + Vector2Int.right,
            cell + Vector2Int.up + Vector2Int.left,
            cell + Vector2Int.down + Vector2Int.right,
            cell + Vector2Int.down + Vector2Int.left,
        };
    }
}
