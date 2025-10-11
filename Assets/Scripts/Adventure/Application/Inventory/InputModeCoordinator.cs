using Adventure.Presentation.Mushroom;
using Adventure.Settings.ViewModel;
using System;
using UniRx;
using Zenject;

public sealed class InputModeCoordinator : IInitializable, IDisposable
{
    private readonly IInputModeService _inputMode;
    private readonly MushroomBookViewModel _bookVM;
    private readonly GameSettingsViewModel _settingsVM;
    private readonly CursorService _cursorService;
    public InputModeCoordinator(
        IInputModeService inputMode,
        MushroomBookViewModel bookVM,
        GameSettingsViewModel settingsVM,
        CursorService cursorService)
    {
        _inputMode = inputMode;
        _bookVM = bookVM;
        _settingsVM = settingsVM;
        _cursorService = cursorService;
        Initialize();
    }
    public void Initialize()
    {
        _bookVM.IsOpen.Subscribe(OnBookStateChanged);
        _settingsVM.IsOpen.Subscribe(OnSettingsStateChanged);
    }

    private void OnBookStateChanged(bool isOpen)
    {
        if (isOpen)
        {
            _inputMode.PushMode(InputMode.OnlyMove); // курсор виден, движение можно

        }
        else
        {
            _inputMode.PopMode(InputMode.OnlyMove);
        }
    }

    private void OnSettingsStateChanged(bool isOpen)
    {
        if (isOpen)
            _inputMode.PushMode(InputMode.Blocked);
        else
            _inputMode.PopMode(InputMode.Blocked);
    }

    public void Dispose()
    {
        //_bookVM.IsOpen.Dispose();
        //_settingsVM.IsOpen.Dispose();
    }
    
}
