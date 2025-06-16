using System.Collections.Generic;
using System;
using UnityEngine;
using static UnityEngine.UI.Image;

public class PerCellGridRenderer : ICellGridRenderer
{
    List<GameObject> instances = new List<GameObject>();
    GameObject prefab;
    GameObject parent;
    float ySize = 1f;
    Grid<GameObject> grid;
    public PerCellGridRenderer(GameObject prefab, GameObject parent)
    {
        this.parent = parent;
        this.prefab = prefab;
    }

    public void Clear()
    {
        foreach (GameObject child in instances)
        {
            GameObject.Destroy(child);
        }
    }

    public void Render(int width, int height, float cellSize,Vector3 origin, float padding)
    {
        Clear();
        grid = new Grid<GameObject>(width, height, cellSize, origin, padding, CreateCellView);
    }

    public Vector3 ToWorld(int x, int y)
    {
        return grid.GetWorldPosition(x, y);
    }

    private GameObject CreateCellView(Grid<GameObject> grid, int x,int y)
    {
        GameObject gameObject =  GameObject.Instantiate(prefab, grid.GetWorldPosition(x, y), Quaternion.identity, parent.transform);
        gameObject.transform.localScale = new Vector3(grid.GetCellSize(), ySize, grid.GetCellSize());
        instances.Add(gameObject);
        return gameObject;
    }
}
