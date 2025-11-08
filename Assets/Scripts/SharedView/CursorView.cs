using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

public sealed class CursorView
{
    private ICursorViewModel _vm;
    private Dictionary<CursorVisualState, Texture2D> _cursorTextrures;
    [Inject]
    public void Construct(List<CursorStateTexture> cursorStateTextures, ICursorViewModel cursorViewModel)
    {
        _cursorTextrures = cursorStateTextures.ToDictionary(x => x.state, y => y.texture);
        _vm = cursorViewModel;
        _vm.CursorState.Subscribe(OnCursorStateChanged);
        _vm.IsLocked.Subscribe(OnLockChanged);
        OnCursorStateChanged(_vm.CursorState.Value);
        OnLockChanged(_vm.IsLocked.Value);
    }

    private void OnCursorStateChanged(CursorVisualState state)
    {
        if (state == CursorVisualState.Hidden)
        {
            Cursor.visible = false;
        }
        else
        {
            var texture = _cursorTextrures[state];
            Cursor.visible = true;
            SetCursor(texture);
        }
    }

    private void OnLockChanged(bool locked)
    {
        if(locked)
            Lock();
        else
            Unlock();
    }
    public void Lock()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Unlock()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    private void SetCursor(Texture2D cursorTexture)
    {
        if (cursorTexture == null)
        {
            Debug.LogWarning("Attempting to set null cursor texture");
            return;
        }
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
    }
}