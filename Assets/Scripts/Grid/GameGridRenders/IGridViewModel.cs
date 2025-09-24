using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGridViewModel
{
   Action<PreviewResult> PreviewChanged { get; set; }
}
