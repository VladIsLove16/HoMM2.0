using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGridViewModel
{
    Action<GridXZ<GameCell>> GridInited { get; set; }
    Action<PreviewResult> PreviewChanged { get; set; }
}
