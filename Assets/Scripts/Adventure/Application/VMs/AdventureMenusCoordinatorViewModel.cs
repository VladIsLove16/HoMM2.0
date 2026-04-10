using Adventure.Application.Dialog;
using Adventure.Infrastructure.Dialog;
using Adventure.Presentation.Mushroom;
using Adventure.Settings.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Zenject;

public class AdventureMenusCoordinatorViewModel
{
    private readonly List<IAdventureGameActiveMenu> _activeMenus = new();
    private readonly IInputModeVM _inputModeVM;

    [Inject]
    public AdventureMenusCoordinatorViewModel(
        IInputModeVM inputModeVM,
        IEnumerable<IAdventureGameActiveMenu> menus)
    {
        _inputModeVM = inputModeVM ?? throw new ArgumentNullException(nameof(inputModeVM));
        if (!menus.Any())
            throw new ArgumentException("No menus registered in AdventureMenusCoordinatorViewModel");

        foreach (var menu in menus)
        {
            if(menu == null)
                throw new ArgumentException("Null menu registered in AdventureMenusCoordinatorViewModel");
            if (menu.IsOpen == null)
                throw new ArgumentException($"Menu {menu.GetType().Name} has null IsOpen property");
            menu.IsOpen
                .Skip(1)
                .Subscribe(_ => OnMenuStateChanged(menu));

            if (menu.IsOpen.Value)
            {
                OnMenuStateChanged(menu);
            }
        }
    }

    private void OnMenuStateChanged(IAdventureGameActiveMenu activeMenu)
    {
        if (activeMenu.IsOpen.Value)
        {
            if (_activeMenus.Contains(activeMenu))
                return;

            _activeMenus.Add(activeMenu);
            _inputModeVM.PushMode(activeMenu.InputMode);
        }
        else
        {
            if (_activeMenus.Remove(activeMenu))
            {
                _inputModeVM.PopMode(activeMenu.InputMode);
            }
        }
    }

    internal void CloseTopmost()
    {
        if (!_activeMenus.Any())
            return;

        var menu = _activeMenus[^1];
        menu.Close();
        // Close() will trigger OnMenuStateChanged, which removes/pop mode.
    }

    public bool HasAnyOpen => _activeMenus.Any();
}
