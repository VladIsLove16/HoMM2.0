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
    [SerializeField] private List<CursorStateTexture> cursorStateTextures;
    private Dictionary<CursorVisualState, Texture2D> _cursorTextrures;
    private ActionResolver _actionResolver;
    [Inject]
    public void Construct(ActionResolver actionResolver)
    {
        _actionResolver = actionResolver;
        actionResolver.ActionResolved += OnActionPreviewChanged;
        actionResolver.ActionNotResolved += OnActionNotResolved;
        _cursorTextrures = cursorStateTextures.ToDictionary(x => x.state, y => y.texture);
        SetDefaultCursor();
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
    /// Скрыть/показать курсор
    /// </summary>
    public void SetCursorVisibility(bool isVisible)
    {
        Cursor.visible = isVisible;
    }
    // Расширенный сервис
    public void SetCursorState(CursorVisualState state)
    {
        if (_cursorTextrures.TryGetValue(state, out var tex))
        {
            SetCursor(tex);
        }
        else
            Debug.LogWarning("no cursor texture for state " + state.ToString());
    }
    public void LockToCenter()
    {
        Cursor.lockState = CursorLockMode.Locked;
        SetCursorState(CursorVisualState.Point);
    }
    public void Unlock()
    {
        Cursor.lockState = CursorLockMode.None;
        SetCursorState(CursorVisualState.Default);
    }
    private void OnActionNotResolved()
    {
        SetCursorState(CursorVisualState.Default);
    }

    private void OnActionPreviewChanged((IActionHandler, ActionContext) tuple)
    {
        Debug.Log("[cursor service] OnActionPreviewChanged " + tuple.Item1.ToString());
        var state = GetCursorStateByActionHandler(tuple.Item1);
        SetCursorState(state);
    }

    private CursorVisualState GetCursorStateByActionHandler(IActionHandler actionHandler)
    {
        switch (actionHandler.ActionType)
        {
            case ActionType.Move:
                return CursorVisualState.Move;
            case ActionType.MoveThenAttack:
            case ActionType.Attack:
                return CursorVisualState.Attack;
            case ActionType.RangedAttack:
                return CursorVisualState.RangedAttack;
            default:
                return CursorVisualState.Default;
        }
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

   
    private void Dispose()
    {
        if (_actionResolver != null)
        {
            _actionResolver.ActionResolved-=OnActionPreviewChanged;
        }
    }

}
