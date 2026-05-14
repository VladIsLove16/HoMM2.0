using System;
using Adventure.Infrastructure.Persistence;
using Adventure.Settings.ViewModel;
using Game.Achievements;
using UniRx;
using UnityEngine;
using UnityEngine.Localization.Settings;

public sealed class MainMenuViewModel : IDisposable, IAchievementsNavigation
{
    private readonly SinglePlayerStartConfigurationSO _singlePlayerStartConfiguration;

    private readonly ReactiveProperty<MainMenuScreen> _activeScreen = new(MainMenuScreen.Primary);
    private MainMenuGameSettingsViewModel _settingsViewModel;
    private AchievementsViewModel _achievementsViewModel;

    public IReadOnlyReactiveProperty<MainMenuScreen> ActiveScreen => _activeScreen;
    public bool CanOpenSettings => _settingsViewModel != null;
    public bool CanOpenAchievements => _achievementsViewModel != null;

    public MainMenuViewModel(SinglePlayerStartConfigurationSO singlePlayerStartConfiguration)
    {
        _singlePlayerStartConfiguration = singlePlayerStartConfiguration;
        ApplySavedLocale();
    }

    [Zenject.Inject]
    private void InjectOverlayViewModels(
        [Zenject.InjectOptional] MainMenuGameSettingsViewModel settingsViewModel = null,
        [Zenject.InjectOptional] AchievementsViewModel achievementsViewModel = null)
    {
        _settingsViewModel = settingsViewModel;
        _achievementsViewModel = achievementsViewModel;
    }

    public void Dispose()
    {
        _activeScreen.Dispose();
    }

    public void StartSinglePlayer()
    {
        if (_singlePlayerStartConfiguration == null)
        {
            Debug.LogError("[MainMenuViewModel] SinglePlayerStartConfiguration is not assigned. Cannot start single player.");
            return;
        }

        if (_singlePlayerStartConfiguration.StartMode == SinglePlayerStartMode.DirectBattle)
        {
            StartSinglePlayerWithBattle();
            return;
        }

        ApplyAdventureStartConfiguration();
        SceneLoader.Load(_singlePlayerStartConfiguration.AdventureScene);
    }

    public void OpenNetworkLobby()
    {
        CloseOverlayPanels();
        GameLaunchPreferences.SetMultiplayerStartScene(SceneLoader.Scene.Adventure);
        _activeScreen.Value = MainMenuScreen.NetworkConnection;
    }

    public void ToggleSettings()
    {
        if (_settingsViewModel == null)
        {
            Debug.LogWarning("[MainMenuViewModel] Settings view model is not available.");
            return;
        }

        ReturnToPrimaryScreen();

        if (_settingsViewModel.IsOpen.Value)
        {
            _settingsViewModel.Close();
            return;
        }

        _achievementsViewModel?.Close();
        _settingsViewModel.Open();
    }
    public void ToggleAChievements()
    {
        if (!_achievementsViewModel.IsOpen.Value)
            OpenAchievements();
        else
            CloseAchievements();
    }
    public void OpenAchievements()
    {
        if (_achievementsViewModel == null)
        {
            Debug.LogWarning("[MainMenuViewModel] Achievements view model is not available.");
            return;
        }

        ReturnToPrimaryScreen();
        _achievementsViewModel.Open();
    }
    public void CloseAchievements()
    {
        if (_achievementsViewModel == null)
        {
            Debug.LogWarning("[MainMenuViewModel] Achievements view model is not available.");
            return;
        }

        ReturnToPrimaryScreen();
        _achievementsViewModel.Close();
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    private void StartSinglePlayerWithBattle()
    {
        _singlePlayerStartConfiguration.ApplyDirectBattleStart();
        SceneLoader.Load(_singlePlayerStartConfiguration.BattleScene);
    }

    private void ApplyAdventureStartConfiguration()
    {
        _singlePlayerStartConfiguration.ApplyAdventureStart();
    }

    private void CloseOverlayPanels()
    {
        _achievementsViewModel?.Close();
        _settingsViewModel?.Close();
    }

    private void ReturnToPrimaryScreen()
    {
        if (_activeScreen.Value != MainMenuScreen.Primary)
            _activeScreen.Value = MainMenuScreen.Primary;
    }

    private static void ApplySavedLocale()
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();

        var locales = LocalizationSettings.AvailableLocales?.Locales;
        if (locales == null || locales.Count == 0)
            return;

        var storage = new JsonFileStorage();
        var data = storage.Load<GameSettingsSaveData>("game-settings");
        if (data == null)
            return;

        var index = Mathf.Clamp(data.LanguageIndex, 0, locales.Count - 1);
        var targetLocale = locales[index];
        if (targetLocale != null && LocalizationSettings.SelectedLocale != targetLocale)
        {
            LocalizationSettings.SelectedLocale = targetLocale;
        }
    }

}
