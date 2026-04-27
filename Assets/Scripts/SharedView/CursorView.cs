using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

public sealed class CursorView : IDisposable
{
    private ICursorViewModel _vm;
    private Dictionary<CursorVisualState, Texture2D> _cursorTextrures;
    private CompositeDisposable _subscriptions;
    private readonly HashSet<CursorVisualState> _missingStates = new();

    [Inject]
    public void Construct(List<CursorStateTexture> cursorStateTextures, ICursorViewModel cursorViewModel)
    {
        if (cursorStateTextures == null)
        {
            throw new ArgumentNullException(nameof(cursorStateTextures));
        }

        _cursorTextrures = cursorStateTextures.ToDictionary(x => x.state, y => y.texture);
        _vm = cursorViewModel;
        _subscriptions = new CompositeDisposable();
        _vm.CursorState.Subscribe(OnCursorStateChanged).AddTo(_subscriptions);
        _vm.IsLocked.Subscribe(OnLockChanged).AddTo(_subscriptions);
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
            if (!_cursorTextrures.TryGetValue(state, out var texture))
            {
                if (_missingStates.Add(state))
                {
                    Debug.LogWarning($"Cursor texture for state '{state}' is not configured. Using default cursor.");
                }
            }
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
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            return;
        }
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
    }

    public void Dispose()
    {
        _subscriptions?.Dispose();
        _subscriptions = null;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
