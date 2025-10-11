using UnityEngine;
/// <summary>
/// Сервис для управления курсором
/// </summary>
public interface ICursorService
{
    void SetDefaultCursor();
    void SetCursorState(CursorVisualState state);
    void SetCursorVisibility(bool isVisible);
    void LockToCenter();
    void Unlock();
}
