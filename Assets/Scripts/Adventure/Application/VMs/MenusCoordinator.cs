using Adventure.Presentation.Mushroom;
using Adventure.Settings.ViewModel;
using System.Collections.Generic;
using Zenject;

public class MenusCoordinatorViewModel : IInitializable
{
    private readonly List<IActiveMenu> _activeMenus = new();
    private readonly InputModeViewModel _inputModeVM;
    private readonly GameSettingsViewModel _settingsVM;
    private readonly MushroomBookViewModel _bookVM;

    [Inject]
    public MenusCoordinatorViewModel(
        InputModeViewModel inputModeVM,
        GameSettingsViewModel settingsVM,
        MushroomBookViewModel bookVM)
    {
        _inputModeVM = inputModeVM;
        _settingsVM = settingsVM;
        _bookVM = bookVM;
    }

    public void Initialize()
    {
        _settingsVM.IsOpen.Subscribe(OnMenuStateChanged);
        _bookVM.IsOpen.Subscribe(OnMenuStateChanged);
    }

    private void OnMenuStateChanged(bool _)
    {
        UpdateMenuState();
    }

    private void UpdateMenuState()
    {
        _activeMenus.Clear();
        if (_settingsVM.IsOpen.Value) _activeMenus.Add(_settingsVM);
        if (_bookVM.IsOpen.Value) _activeMenus.Add(_bookVM);

        if (_activeMenus.Count == 0)
        {
            _inputModeVM.PushMode(InputMode.Enabled);
        }
        else
        {
            // приоритет: настройки блокируют управление
            if (_settingsVM.IsOpen.Value)
            {
                _inputModeVM.PushMode(InputMode.Blocked);
            }
            else if (_bookVM.IsOpen.Value)
            {
                _inputModeVM.PushMode(InputMode.Enabled);
            }
        }
    }

    public void CloseFirst()
    {
        if (_settingsVM.IsOpen.Value)
            _settingsVM.Close();
        else if (_bookVM.IsOpen.Value)
            _bookVM.Close();
    }

    public bool HasAnyOpen => _settingsVM.IsOpen.Value || _bookVM.IsOpen.Value;
}