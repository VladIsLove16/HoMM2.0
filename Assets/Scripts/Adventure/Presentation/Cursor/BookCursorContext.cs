public sealed class BookCursorContext : ICursorContext
{
    public string Name => "Book";
    public CursorVisualState DesiredCursorState => CursorVisualState.Default;
    public bool IsExclusive => true; // блокирует игрока
}
