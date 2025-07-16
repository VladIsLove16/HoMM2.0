using UnityEngine;
/// <summary>
/// Сервис для управления курсором
/// </summary>
public interface ICursorService
{
    void SetDefaultCursor();
    void SetCursorState(CursorState state);
    void SetCursorVisibility(bool isVisible);
}
