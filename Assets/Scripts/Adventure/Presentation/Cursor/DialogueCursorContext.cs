public sealed class DialogueCursorContext : ICursorContext
{
    public string Name => "Dialogue";
    public CursorVisualState DesiredCursorState => CursorVisualState.Default;
    public bool IsExclusive => true;
}
