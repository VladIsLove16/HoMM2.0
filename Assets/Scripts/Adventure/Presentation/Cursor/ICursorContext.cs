public interface ICursorContext
{
    string Name { get; }
    CursorVisualState DesiredCursorState { get; }
    bool IsExclusive { get; } // блокирует ли другие контексты
}
