using UnityEngine;

public class CityWall : IGridContent, IBlockable, IBlocksLineOfSight
{
    public bool BlocksSight => true;

    public Vector2Int Position { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public GridContentType GridContentType => throw new System.NotImplementedException();

    public bool IsBlueTeam => throw new System.NotImplementedException();

    public bool CanMoveThrough() => false;
}
