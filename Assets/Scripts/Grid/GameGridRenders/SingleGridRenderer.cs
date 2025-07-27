using System.Collections.Generic;
using UnityEngine;

public class SingleGridRenderer : IGridCellRenderer
{
    private GameObject prefab;
    private GameObject singleObject;
    private float ySize = 1f;
    private float cellSize;
    private float padding;
    public SingleGridRenderer(GameObject prefab, Material cellSelectMaterial)
    {
        this.prefab = prefab;
    }
    public void Render(int width, int height, float cellSize, Vector3 origin, float padding)
    {
        this.cellSize = cellSize;
        this.padding = padding;
        float x = width * (cellSize + padding);
        float y = height * (cellSize + padding);
        singleObject = GameObject.Instantiate(prefab);
        singleObject.transform.localScale = new Vector3(x, ySize, y);
        singleObject.transform.position = origin;
    }

    public void Clear()
    {
        if (singleObject != null)
            GameObject.Destroy(singleObject);
    }

    public Vector3 ToWorld(int x, int y)
    {
        Vector3 origin = singleObject.transform.position;
        float cellOffset = cellSize + padding;
        return origin + new Vector3(cellOffset *x, ySize, cellOffset *y);
    }

    public Vector3 ToWorld(Vector2Int vector2Int)
    {
      return ToWorld(vector2Int.x, vector2Int.y);
    }

    public Vector2Int ToGrid(Vector3 position)
    {
        throw new System.NotImplementedException();
    }

    public void SetSelectableState(Vector2Int cellCoords, bool v)
    {
        throw new System.NotImplementedException();
    }

    public bool ToGrid(Vector3 position, out Vector2Int coords)
    {
        throw new System.NotImplementedException();
    }

    public void SetCellState(Vector2Int cellCoords, CellState v)
    {
        throw new System.NotImplementedException();
    }

    public CellState GetCellState(Vector2Int coords)
    {
        throw new System.NotImplementedException();
    }

    public void ReturnState(Vector2Int hoveredCell)
    {
        throw new System.NotImplementedException();
    }

    public CellState GetPrevState(Vector2Int hoveredCell)
    {
        throw new System.NotImplementedException();
    }

    public CellState[] GetCellStates(Vector2Int coords)
    {
        throw new System.NotImplementedException();
    }

    public void AddState(Vector2Int coords, CellState hovered)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveState(Vector2Int hoveredCell, CellState hovered)
    {
        throw new System.NotImplementedException();
    }

    public void AddStates(List<Vector2Int> points, CellState state)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveStates(CellState state)
    {
        throw new System.NotImplementedException();
    }
}
