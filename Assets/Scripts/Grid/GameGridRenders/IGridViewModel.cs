using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGridViewModel
{
   Action<Dictionary<CellState, List<Vector2Int>>> PreviewChanged { get; set; }
}
