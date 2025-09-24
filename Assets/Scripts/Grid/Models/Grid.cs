
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

    internal bool IsInBounds(Vector2Int vector2Int)
    {
       return vector2Int.x<=width && vector2Int.y<=height && vector2Int.x>=0 && vector2Int.y >=0;
    }
}
