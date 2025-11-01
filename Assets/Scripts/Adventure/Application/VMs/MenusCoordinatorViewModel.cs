using Adventure.Application.Dialog;
using Adventure.Infrastructure.Dialog;
using Adventure.Presentation.Mushroom;
using Adventure.Settings.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Zenject;

public class MenusCoordinatorViewModel
{
    private readonly List<IActiveMenu> _activeMenus = new();
    private readonly IInputModeVM _inputModeVM;
    [Inject]
    public MenusCoordinatorViewModel(
        IInputModeVM inputModeVM,
        IEnumerable<IActiveMenu> menus)
    {
        _inputModeVM = inputModeVM;
        if(menus.Count() == 0)
            throw new ArgumentException("No menus registered in MenusCoordinatorViewModel");
        foreach (var menu in menus)
        {
            menu.IsOpen
                .Skip(1)
                .Subscribe(_ => OnMenuStateChanged(menu));

            if (menu.IsOpen.Value)
            {
                OnMenuStateChanged(menu);
            }
        }
    }
    private void OnMenuStateChanged(IActiveMenu activeMenu)
    {
        if (activeMenu.IsOpen.Value == true)
        {
            _inputModeVM.PushMode(activeMenu.InputMode);
            _activeMenus.Add(activeMenu);
        }
        else
        {
            _inputModeVM.PopMode(activeMenu.InputMode);
            _activeMenus.Remove(activeMenu);
        }
    }

    internal void CloseFirst()
    {
        if(!_activeMenus.Any())
            return;
        var menu = _activeMenus.First();
        menu.Close();
        _activeMenus.Remove(menu);
    }

    public bool HasAnyOpen => _activeMenus.Any();
}
