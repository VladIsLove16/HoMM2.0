using UnityEngine;

public static class GridUtils 
{
    private static TextMesh[,] debugTextArray;
    public static void DrawDebugText(int width, int height,Grid<GameObject> grid ) 
    {
        debugTextArray = new TextMesh[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Debug.DrawLine(grid.GetWorldPosition(x, y), grid.GetWorldPosition(x, y + 1), Color.white, 100f);
                Debug.DrawLine(grid.GetWorldPosition(x, y), grid.GetWorldPosition(x + 1, y), Color.white, 100f);
            }
        }
        Debug.DrawLine(grid.GetWorldPosition(0, height), grid.GetWorldPosition(width, height), Color.white, 100f);
        Debug.DrawLine(grid.GetWorldPosition(width, 0), grid.GetWorldPosition(width, height), Color.white, 100f);

        grid.OnGridObjectChanged += (object sender, Grid<GameObject>.OnGridObjectChangedEventArgs eventArgs) => {
            debugTextArray[eventArgs.x, eventArgs.y].text = grid.GetGridObject(eventArgs.x, eventArgs.y)?.ToString();
        };
    }
}
