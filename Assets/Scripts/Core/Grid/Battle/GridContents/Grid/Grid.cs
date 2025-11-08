
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid<TGridObject> {

    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs {
        public int x;
        public int y;
    }

    private int width;
    private int height;
    private float cellSize;
    private float padding = 0f;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;

    public Grid(int width, int height, float cellSize, Vector3 originPosition, float padding, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.padding = padding;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }
    }
    public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject) {
        
    }

    public int GetWidth() {
        return width;
    }

    public int GetHeight() {
        return height;
    }

    public float GetCellSize() {
        return cellSize;
    }

    public Vector3 GetWorldPosition(int x, int y) {
        return new Vector3(x, 0, y) * (cellSize + padding) + originPosition;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y) {
        float xPos =( (worldPosition - originPosition).x + (cellSize / 2 + padding / 2)) / (cellSize + padding) ;
        float yPos = ((worldPosition - originPosition).z + (cellSize / 2 + padding / 2)) / (cellSize + padding) ;
        x = Mathf.FloorToInt(xPos);
        y = Mathf.FloorToInt(yPos);
    }

    public void SetGridObject(int x, int y, TGridObject value) {
        if (x >= 0 && y >= 0 && x < width && y < height) {
            gridArray[x, y] = value;
            TriggerGridObjectChanged(x, y);
        }
    }

    public void TriggerGridObjectChanged(int x, int y) {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value) {
        GetXY(worldPosition, out int x, out int y);
        SetGridObject(x, y, value);
    }

    public TGridObject GetGridObject(int x, int y) {
        if (x >= 0 && y >= 0 && x < width && y < height) {
            return gridArray[x, y];
        } else {
            return default(TGridObject);
        }
    }

    public TGridObject GetGridObject(Vector3 worldPosition) {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }

    public Vector2Int GetXY(Vector3 worldPosition) {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return new Vector2Int(x,y);
    }
    public TGridObject[,] GetGridObjects()
    {
        return gridArray;
    }

    public bool IsInBounds(Vector2Int vector2Int)
    {
        return vector2Int.x >= 0 && vector2Int.x < width &&
               vector2Int.y >= 0 && vector2Int.y < height;
    }
    public List<Vector2Int> GetNeighbors(Vector2Int cell, int width, int height, bool includeDiagonals = false)
    {
        var neighbors = new List<Vector2Int>();

        // Четыре направления: вверх, вниз, влево, вправо
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),  // вверх
            new Vector2Int(0, -1), // вниз
            new Vector2Int(-1, 0), // влево
            new Vector2Int(1, 0),  // вправо
        };

        // Диагональные направления
        Vector2Int[] diagonals = new Vector2Int[]
        {
            new Vector2Int(-1, 1),  // верх-лево
            new Vector2Int(1, 1),   // верх-право
            new Vector2Int(-1, -1), // низ-лево
            new Vector2Int(1, -1)   // низ-право
        };

        foreach (var dir in directions)
        {
            Vector2Int neighbor = cell + dir;
            if (IsInBounds(neighbor))
                neighbors.Add(neighbor);
        }

        // Добавляем диагональные, если нужно
        if (includeDiagonals)
        {
            foreach (var dir in diagonals)
            {
                Vector2Int neighbor = cell + dir;
                if (IsInBounds(neighbor))
                    neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }
    public Vector2Int GetClosestNeighbor(Vector3 worldPosition, bool includeDiagonals = false)
    {
        // Получаем координаты клетки, в которой находится точка
        Vector2Int cell = GetXY(worldPosition);

        // Получаем соседей
        List<Vector2Int> neighbors = GetNeighbors(cell, width, height, includeDiagonals);

        if (neighbors.Count == 0)
            return cell; // если соседей нет, возвращаем саму клетку

        Vector2Int closest = neighbors[0];
        float minDistance = Vector3.Distance(worldPosition, GetWorldPosition(closest.x, closest.y));

        foreach (var neighbor in neighbors)
        {
            float distance = Vector3.Distance(worldPosition, GetWorldPosition(neighbor.x, neighbor.y));
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = neighbor;
            }
        }

        return closest;
    }
}
