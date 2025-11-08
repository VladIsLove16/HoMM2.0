using Adventure.Settings.ViewModel;
using System;
using UniRx;
using UnityEngine;

/// <summary>
/// ОШИБОЧНЫЙ КОД! Переписать на MVVM
/// </summary>
public class HelpMenu : MonoBehaviour, IAdventureGameActiveMenu
{
    private ReactiveProperty<bool> isOpen = new(false);
    public InputMode InputMode => InputMode.Enabled;
    public IReadOnlyReactiveProperty<bool> IsOpen => isOpen;
    private void Awake()
    {
        IsOpen.Subscribe(SetState);
    }
    internal void Toggle()
    {
        SetState(!isOpen.Value);
    }
    private void SetState(bool state)
    {
        isOpen.SetValueAndForceNotify(state);
        if (state)
            Open();
        else
            Close();
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Open()
    {
       gameObject.SetActive(!isOpen.Value);
    }

    
}
