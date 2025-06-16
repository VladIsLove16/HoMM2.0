using System;
using UnityEngine;
public partial class GridXZ<TGridObject> {

    public event EventHandler<GridCellChangedEventArgs> OnGridObjectChanged;
    
    private int width;
    private int height;
    protected TGridObject[,] gridArray;
    public GridXZ(int width, int height, Func<GridXZ<TGridObject>, int, int, TGridObject> createGridObject) {
        this.width = width;
        this.height = height;

        gridArray = new TGridObject[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++) {
            for (int z = 0; z < gridArray.GetLength(1); z++) {
                gridArray[x, z] = createGridObject(this, x, z);
            }
        }
    }

    public int GetWidth() {
        return width;
    }

    public int GetHeight() {
        return height;
    }

   

    public void SetGridObject(int x, int z, TGridObject value) {
        if (x >= 0 && z >= 0 && x < width && z < height) {
            gridArray[x, z] = value;
            TriggerGridObjectChanged(x, z);
        }
    }

    public void TriggerGridObjectChanged(int x, int z) {
        OnGridObjectChanged?.Invoke(this, new GridCellChangedEventArgs { x = x, y = z });
    }

    public bool TryGetGridObject(int x, int z, out TGridObject result)
    {
        if (IsInBounds(x, z))
        {
            result = gridArray[x, z];
            return true;
        }

        result = default;
        return false;
    }

    public void ClearGridObject(int x, int z)
    {
        if (!IsInBounds(x, z))
            throw new ArgumentOutOfRangeException();

        gridArray[x, z] = default;
        TriggerGridObjectChanged(x, z);
    }

    public bool IsInBounds(int x, int z)
    {
        return x >= 0 && z >= 0 && x < width && z < height;
    }

    public TGridObject GetGridObject(int x, int z)
    {
        if (!IsInBounds(x, z))
            throw new ArgumentOutOfRangeException($"GameGridModel index ({x},{z}) is out of bounds ({width},{height})");

        return gridArray[x, z];
    }


    public TGridObject[,] GetGridArray()
    {
        return gridArray;
    }



    public Vector2Int ValidateGridPosition(Vector2Int gridPosition) {
        return new Vector2Int(
            Mathf.Clamp(gridPosition.x, 0, width - 1),
            Mathf.Clamp(gridPosition.y, 0, height - 1)
        );
    }

    public bool IsValidGridPosition(Vector2Int gridPosition) {
        int x = gridPosition.x;
        int z = gridPosition.y;

        if (x >= 0 && z >= 0 && x < width && z < height) {
            return true;
        } else {
            return false;
        }
    }

    public bool IsValidGridPositionWithPadding(Vector2Int gridPosition) {
        Vector2Int padding = new Vector2Int(2, 2);
        int x = gridPosition.x;
        int z = gridPosition.y;

        if (x >= padding.x && z >= padding.y && x < width - padding.x && z < height - padding.y) {
            return true;
        } else {
            return false;
        }
    }

}


//bool showDebug = false;
//if (showDebug) {
//    TextMesh[,] debugTextArray = new TextMesh[width, height];

//    for (int x = 0; x < gridArray.GetLength(0); x++) {
//        for (int y = 0; y < gridArray.GetLength(1); y++) {
//            //debugTextArray[x, y] = UtilsClass.CreateWorldText(gridArray[x, y]?.ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize, 0, cellSize) * .5f, 40, Color.white, TextAnchor.MiddleCenter, TextAlignment.Center);
//            debugTextArray[x, y].transform.localScale = Vector3.one * .13f;
//            debugTextArray[x, y].transform.eulerAngles = new Vector3(90, 0, 0);
//            Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
//            Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
//        }
//    }
//    Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
//    Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

//    OnGridObjectChanged += (object sender, GridCellChangedEventArgs eventArgs) => {
//        debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
//    };
//}