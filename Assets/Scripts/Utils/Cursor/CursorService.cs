using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
/// <summary>
/// Реализация сервиса работы с курсором
/// </summary>
public class CursorService : ICursorService
{
    private Texture2D _currentCursor;
    private Vector2 _currentHotspot;
    private Dictionary<CursorState, Texture2D> _cursorTextrures;
    private ActionResolver _actionResolver;
    
    public CursorService(List<CursorStateTexture> cursorStateTextures)
    {
        _cursorTextrures = cursorStateTextures.ToDictionary(x => x.state,y=> y.texture);
        SetDefaultCursor();
    }
    
    [Inject]
    public void Construct(ActionResolver actionResolver)
    {
        _actionResolver = actionResolver;
        actionResolver.ActionResolved += OnActionPreviewChanged;
    }

    private void OnActionPreviewChanged((IActionHandler, ActionContext) tuple)
    {
        SetCursorState(tuple.Item1.CanExecute(tuple.Item2) ?
            CursorState.ActionAvailable : CursorState.ActionNotAvailable);
    }

    /// <summary>
    /// Установить курсор по умолчанию (системный)
    /// </summary>
    public void SetDefaultCursor()
    {
        _currentCursor = null;
        _currentHotspot = Vector2.zero;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    /// <summary>
    /// Установить пользовательский курсор
    /// </summary>
    /// <param name="cursorTexture">Текстура курсора</param>
    /// <param name="hotspot">Точка взаимодействия (относительно верхнего левого угла)</param>
    private void SetCursor(Texture2D cursorTexture, Vector2 hotspot = default)
    {
        if (cursorTexture == null)
        {
            Debug.LogWarning("Attempting to set null cursor texture");
            return;
        }

        _currentCursor = cursorTexture;
        _currentHotspot = hotspot;
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
    }

    /// <summary>
    /// Скрыть/показать курсор
    /// </summary>
    public void SetCursorVisibility(bool isVisible)
    {
        Cursor.visible = isVisible;
    }
    // Расширенный сервис
    public void SetCursorState(CursorState state)
    {
        SetCursor(_cursorTextrures[state]);
    }
    
    public void Dispose()
    {
        if (_actionResolver != null)
        {
            _actionResolver.ActionResolved-=OnActionPreviewChanged;
        }
    }

}
