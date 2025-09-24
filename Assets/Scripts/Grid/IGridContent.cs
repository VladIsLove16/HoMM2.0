using System.Collections.Generic;
using UnityEngine;

public interface IGridContent
{
    bool IsBlueTeam { get; }
    public Vector2Int Position { get; set; }
    public GridContentType GridContentType { get; }
}
