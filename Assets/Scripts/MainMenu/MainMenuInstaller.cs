using Adventure.Infrastructure.Persistence;
using Adventure.Settings.Model;
using Adventure.Settings.View;
using Adventure.Settings.ViewModel;
using Game.Achievements;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

public sealed class MainMenuInstaller : MonoInstaller
{
    [Header("Scene References")]
    [SerializeField] private MainmenuMainPanel mainMenuUI;
    [SerializeField] private MainMenuSettingsView settingsView;
    [SerializeField] private AchievementsView achievementsView;
    [SerializeField] private AudioMixer audioMixer;

    [Header("Configuration")]
    [SerializeField] private SinglePlayerStartConfigurationSO singlePlayerStartConfiguration;
    [SerializeField] private AchievementCatalog achievementCatalog;

    public override void InstallBindings()
    {
        if (!ValidateReferences())
            return;

        Container.Bind<SinglePlayerStartConfigurationSO>().FromInstance(singlePlayerStartConfiguration).AsSingle();
        BindPersistence();
        BindSettings();
        BindAchievements();
        Container.BindInterfacesAndSelfTo<MainMenuViewModel>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<MainMenuGameSettingsViewModel>().AsSingle().NonLazy();

        Container.Bind<MainmenuMainPanel>().FromInstance(mainMenuUI).AsSingle();
        Container.QueueForInject(mainMenuUI);

        Container.Bind<MainMenuSettingsView>().FromInstance(settingsView).AsSingle();
        Container.QueueForInject(settingsView);
        QueueSettingsSectionsForInject(settingsView);

        Container.Bind<AchievementsView>().FromInstance(achievementsView).AsSingle().NonLazy();
        Container.QueueForInject(achievementsView);
    }

    private void BindPersistence()
    {
        Container.Bind<IJsonFileStorage>().To<JsonFileStorage>().AsSingle();
        Container.Bind<IDataRepository<GameSettingsSaveData>>()
            .To<JsonDataRepository<GameSettingsSaveData>>()
            .AsSingle()
            .WithArguments("game-settings");
        Container.Bind<IDataRepository<GameStateSaveData>>()
            .To<JsonDataRepository<GameStateSaveData>>()
            .AsSingle()
            .WithArguments("game-state");
    }

    private void BindSettings()
    {
        Container.Bind<GameSettingsModel>().FromMethod(_ => GameSettingsRuntimeStore.Resolve()).AsSingle();
        Container.Bind<PauseController>().AsSingle();
        Container.Bind<AudioMixer>().FromInstance(audioMixer).AsSingle();
    }

    private void BindAchievements()
    {
        if (achievementCatalog == null)
        {
            Container.Bind<IAchievementDefinitionProvider>().To<EmptyAchievementDefinitionProvider>().AsSingle();
        }
        else
        {
            Container.Bind<AchievementCatalog>().FromInstance(achievementCatalog).AsSingle();
            Container.Bind<IAchievementDefinitionProvider>().To<AchievementCatalogDefinitionProvider>().AsSingle();
        }

        Container.Bind<IAchievementStorage>().To<PlayerPrefsAchievementStorage>().AsSingle();
        Container.Bind<ICurrencyWallet>().To<GameStateCurrencyWallet>().AsSingle();
        Container.Bind<IAchievementService>().To<AchievementService>().AsSingle();
        Container.BindInterfacesAndSelfTo<AchievementsViewModel>().AsSingle().NonLazy();
    }

    private void QueueSettingsSectionsForInject(MainMenuSettingsView view)
    {
        if (view == null)
            return;

        var sections = view.GetConfiguredSections();
        for (var i = 0; i < sections.Length; i++)
        {
            if (sections[i] != null)
                Container.QueueForInject(sections[i]);
        }
    }

    private bool ValidateReferences()
    {
        if (singlePlayerStartConfiguration == null)
        {
            Debug.LogError("[MainMenuInstaller] SinglePlayerStartConfiguration is not assigned.", this);
            return false;
        }

        if (mainMenuUI == null)
        {
            Debug.LogError("[MainMenuInstaller] MainMenuUI is not assigned.", this);
            return false;
        }

        if (settingsView == null)
        {
            Debug.LogError("[MainMenuInstaller] SettingsView is not assigned.", this);
            return false;
        }

        if (achievementsView == null)
        {
            Debug.LogError("[MainMenuInstaller] AchievementsView is not assigned.", this);
            return false;
        }

        if (audioMixer == null)
        {
            Debug.LogError("[MainMenuInstaller] AudioMixer is not assigned.", this);
            return false;
        }

        return true;
    }
}
