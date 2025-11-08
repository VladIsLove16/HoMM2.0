/// <summary>
/// Indicates that a grid content can block movement on a tile.
/// </summary>
public interface IBlockable
{
    bool CanMoveThrough();
}
