using System;
    public class GridCellUnitSpawnedEventArgs : EventArgs {
        public int x;
        public int y;
        public IGridContent addedContent;
    } 
    public class GridCellContentRemovedEventArgs : EventArgs {
        public int x;
        public int y;
        public IGridContent removedContent;
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

//    GridObjectChanged += (object sender, GridCellUnitSpawnedEventArgs eventArgs) => {
//        debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
//    };
//}