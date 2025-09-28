using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGridViewModel
{
    public event Action<int,int> GridInited;
    public event Action<PreviewResult> PreviewChanged;
    public event Action<PreviewResult> PreviewUpdated;
}
