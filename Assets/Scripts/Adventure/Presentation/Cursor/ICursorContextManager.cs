public interface ICursorContextManager
{
    void RequestCursor(ICursorContext context);
    void ReleaseCursor(ICursorContext context);
    void ForceReset();
}