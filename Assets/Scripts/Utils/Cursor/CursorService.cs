using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
/// <summary>
/// Реализация сервиса работы с курсором
/// </summary>
public class CursorService : MonoBehaviour, ICursorService
{
    private Texture2D _currentCursor;
    private Vector2 _currentHotspot;
    [SerializeField] private List<CursorStateTexture> cursorStateTextures;
    private Dictionary<CursorState, Texture2D> _cursorTextrures;
    private ActionResolver _actionResolver;
    [Inject]
    public CursorService()
    {
        _cursorTextrures = cursorStateTextures.ToDictionary(x => x.state, y => y.texture);
        SetDefaultCursor();
    }
    
    [Inject]
    public void Construct(ActionResolver actionResolver)
    {
        _actionResolver = actionResolver;
        actionResolver.ActionResolved += OnActionPreviewChanged;
        actionResolver.ActionNotResolved += OnActionNotResolved;
    }

    private void OnActionNotResolved()
    {
        SetCursorState(CursorState.Default);
    }

    private void OnActionPreviewChanged((IActionHandler, ActionContext) tuple)
    {
        Debug.Log("[cursor service] OnActionPreviewChanged " + tuple.Item1.ToString());
        var state = GetCursorStateByActionHandler(tuple.Item1);
        SetCursorState(state);
    }

    private CursorState GetCursorStateByActionHandler(IActionHandler actionHandler)
    {
        switch (actionHandler.ActionType)
        {
            case ActionType.Move:
                return CursorState.Move;
            case ActionType.MoveThenAttack:
            case ActionType.Attack:
                return CursorState.Attack;
            case ActionType.RangedAttack:
                return CursorState.RangedAttack;
            default:
                return CursorState.Default;
        }
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
