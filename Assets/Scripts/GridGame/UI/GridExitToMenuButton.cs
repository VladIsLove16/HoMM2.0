using Adventure.Settings.ViewModel;
using UnityEngine;
using Zenject;

/// <summary>
/// Simple handler for a UI button that exits the grid battle back to the main menu.
/// </summary>
public sealed class GridExitToMenuButton : MonoBehaviour
{
    private PauseController _pauseController;

    [Inject]
    public void Construct(PauseController pauseController)
    {
        _pauseController = pauseController;
    }

    public void ExitToMenu()
    {
        // Снимаем паузу, если она была включена настройками.
        _pauseController?.SetPaused(false);

        // Переходим в главное меню через общий загрузчик сцен.
        SceneLoader.Load(SceneLoader.Scene.MainMenu);
    }
}

