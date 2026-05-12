using System;
using Adventure.Infrastructure.Persistence;
using UniRx;
using UnityEngine;
using UnityEngine.Localization.Settings;

public sealed class MainMenuViewModel : IDisposable
{
    private readonly SinglePlayerStartConfigurationSO _singlePlayerStartConfiguration;

    private readonly ReactiveProperty<bool> _isSettingsPanelOpen = new(false);
    private readonly ReactiveProperty<MainMenuScreen> _activeScreen = new(MainMenuScreen.Primary);

    public IReadOnlyReactiveProperty<bool> IsSettingsPanelOpen => _isSettingsPanelOpen;
    public IReadOnlyReactiveProperty<MainMenuScreen> ActiveScreen => _activeScreen;

    public MainMenuViewModel(SinglePlayerStartConfigurationSO singlePlayerStartConfiguration)
    {
        _singlePlayerStartConfiguration = singlePlayerStartConfiguration;
        ApplySavedLocale();
    }

    public void Dispose()
    {
        _isSettingsPanelOpen.Dispose();
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

    public void ToggleSettingsPanel()
    {
        SetSettingsPanelOpen(!_isSettingsPanelOpen.Value);
    }

    public void HideSettingsPanel()
    {
        SetSettingsPanelOpen(false);
    }

    public void OpenNetworkLobby()
    {
        GameLaunchPreferences.SetMultiplayerStartScene(SceneLoader.Scene.Adventure);
        SetSettingsPanelOpen(false);
        _activeScreen.Value = MainMenuScreen.NetworkConnection;
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

    private void SetSettingsPanelOpen(bool visible)
    {
        if (_isSettingsPanelOpen.Value == visible)
            return;

        _isSettingsPanelOpen.Value = visible;
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
