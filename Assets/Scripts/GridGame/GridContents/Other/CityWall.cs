using UnityEngine;

public class CityWall : IGridContent, IBlockable, IBlocksLineOfSight
{
    public bool BlocksSight => true;

    public Vector2Int Position { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public GridContentType GridContentType => throw new System.NotImplementedException();

    public Team Team => Team.Blue; // default placeholder - implement properly when integrating

    public bool CanMoveThrough() => false;
}
