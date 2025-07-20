using UnityEngine;

public interface IGridContent
{
    public Vector2Int Position { get; set; }
    public GridContentType GridContentType { get; }
}
public enum GridContentType
{
    unit,
    barrel,
    rock
}