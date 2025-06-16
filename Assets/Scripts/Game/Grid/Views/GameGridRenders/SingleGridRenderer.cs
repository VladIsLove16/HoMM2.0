using UnityEngine;

public class SingleGridRenderer : ICellGridRenderer
{
    private GameObject prefab;
    private GameObject singleObject;
    private float ySize = 1f;
    private float cellSize;
    private float padding;
    public SingleGridRenderer(GameObject prefab)
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
}
