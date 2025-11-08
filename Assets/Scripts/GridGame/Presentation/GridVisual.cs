using System;
using UnityEngine;

public class GridVisual<TGridObject> : GridXZ<TGridObject> where TGridObject : MonoBehaviour
{
    private float cellSize;
    public float CellSize
    {
    get { return cellSize; }
        set { 
            cellSize = value;
            UpdateVisual();
        }
    }
    private Vector3 padding;
    public Vector3 Padding
    {
    get { return padding; }
        set {
            padding = value;
            UpdateVisual();
        }
    }
    private Vector3 originPosition;
    public GridVisual(int width, int height, Func<GridXZ<TGridObject>, int, int, TGridObject> createGridObject,
       float cellSize = 1f, Vector3 originPosition = new Vector3(), Vector3 padding = new Vector3())
       : base(width, height, createGridObject)
    {
        this.cellSize = cellSize;
        this.originPosition = originPosition;
        this.padding = padding;
    }
    public void UpdateVisual()
    {
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int z = 0; z < gridArray.GetLength(1); z++)
            {
               MonoBehaviour obj = gridArray[x, z];
               obj.transform.position = GetWorldPosition(x, z);
            }
        }
    }

   
    public Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x, 0, z) * cellSize + originPosition;
    }

    public void GetXZ(Vector3 worldPosition, out int x, out int z)
    {
        x = Mathf.RoundToInt((worldPosition - originPosition).x / cellSize);
        z = Mathf.RoundToInt((worldPosition - originPosition).z / cellSize);
    }

    //public void SetGridObject(Vector3 worldPosition, TGridObject value)
    //{
    //    GetXZ(worldPosition, out int x, out int z);
    //    SetGridObject(x, z, value);
    //}

    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        int x, z;
        GetXZ(worldPosition, out x, out z);
        return GetGridObject(x, z);
    }
}
