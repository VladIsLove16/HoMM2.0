using Adventure.Infrastructure.Persistence;
using Adventure.Infrastructure.State;
using CustomEventBus;
using Game.Achievements;
using Game.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Adventure.Settings.Configuration;
using Adventure.Settings.ViewModel;
using Adventure.Settings.Model;
using UnityEngine.Audio;
using Adventure.Settings.View;
using SharedView.Audio;

public class GridGameMonoInstaller : MonoInstaller
{
    [Header("Gameplay References")]
    [SerializeField] private GameController gameController;
    [SerializeField] private GameNetworkCommandGateway networkCommandGateway;
    [SerializeField] private SceneLoadWatcher sceneLoadWatcher;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private GameAudioSettingsSO gameAudioSettings;
    [Header("View model dependencies")]
    [SerializeField] private GridUnitAssetMap gridUnitAssets;
    [SerializeField] private GridRenderSettingsSO gridRenderSettings;
    [SerializeField] private AnimationSpeedSettings animationSpeedSettings;
    [SerializeField] private BattleAiControlConfigSO battleAiControlConfig;
    [Header("Model dependencies")]
    [SerializeField] private StatusEffectDatas statusEffectDatas;
    [SerializeField] private GameConfigurationService gameConfigurationService;
    [Header("Presentation")]
    [SerializeField] private MonoBehaviour _presentationInstaller;
    [SerializeField] private GridGameSettingsView _gridGameSettingsView;
    [SerializeField] private Game.Achievements.AchievementsView _achievementsView;
    [SerializeField] private Game.Achievements.AchievementToastView _achievementToastView;
    [Header("Achievements")]
    [SerializeField] private AchievementCatalog achievementCatalog;
    [Header("Cursor")]
    [SerializeField] private List<CursorStateTexture> cursorStateTextures;
    [SerializeField] private MouseSensitivityProfileSO mouseSensitivityProfile;
    public override void InstallBindings()
    {
        BindConfigurationService();
        BindServices();
        BindModels();
        BindViewModels();
        BindGameController();
        BindView();
    }

    private void BindView()
    {
        BindCursor();
        if (_presentationInstaller is IGamePresentationInstaller gamePresentationInstaller)
            gamePresentationInstaller.Install(Container);
        else
            throw new ArgumentException();
        Container.Bind<GridGameSettingsView>().FromInstance(_gridGameSettingsView).AsSingle();
        if (_achievementsView != null)
        {
            Container.Bind<Game.Achievements.AchievementsView>().FromInstance(_achievementsView).AsSingle().NonLazy();
        }
        else
        {
            Debug.LogWarning("[GridGameMonoInstaller] AchievementsView is not assigned.", this);
        }
        if (_achievementToastView != null)
        {
            Container.Bind<Game.Achievements.AchievementToastView>().FromInstance(_achievementToastView).AsSingle().NonLazy();
        }
        else
        {
            Debug.LogWarning("[GridGameMonoInstaller] AchievementToastView is not assigned.", this);
        }
    }

    private void BindCursor()
    {
        var textures = cursorStateTextures;
        if (textures == null || textures.Count == 0)
        {
            Debug.LogWarning("[GridGameMonoInstaller] CursorStateTextures are not assigned. Using default cursor.");
            textures = new List<CursorStateTexture>();
        }

        Container.BindInterfacesAndSelfTo<CursorView>().AsSingle().WithArguments(textures).NonLazy();
    }

    private void BindConfigurationService()
    {
        Container.BindInterfacesTo<GameConfigurationService>().FromInstance(gameConfigurationService).AsSingle();
    }

    private void BindServices()
    {
        BindPersistenceServices();
        BindAchievementServices();
        BindMouseSensitivity();
        BindAudioServices();
        Container.Bind<GameNetworkCommandGateway>().FromInstance(networkCommandGateway).AsSingle();
        Container.Bind<SceneLoadWatcher>().FromInstance(sceneLoadWatcher).AsSingle();
        Container.Bind<IBattleAnimationGate>().To<BattleAnimationGate>().AsSingle();
        Container.Bind<IAnimationSpeedSettings>().To<AnimationSpeedSettings>().FromInstance(animationSpeedSettings).AsSingle().WithArguments(AnimationSpeedMode.Fast);
        if (battleAiControlConfig == null)
        {
            Debug.LogWarning("[GridGameMonoInstaller] BattleAiControlConfig is not assigned. Using runtime default config.");
            battleAiControlConfig = ScriptableObject.CreateInstance<BattleAiControlConfigSO>();
        }
        Container.Bind<BattleAiControlConfigSO>().FromInstance(battleAiControlConfig).AsSingle();
        if (gridRenderSettings == null)
        {
            Debug.LogWarning("[GridGameMonoInstaller] GridRenderSettings is not assigned. Using default settings.");
            var runtimeSettings = ScriptableObject.CreateInstance<GridRenderSettingsSO>();
            Container.Bind<IGridRenderSettings>().FromInstance(runtimeSettings).AsSingle();
        }
        else
        {
            Container.Bind<IGridRenderSettings>().FromInstance(gridRenderSettings).AsSingle();
        }
        Container.Bind<EventBus>().AsSingle();
        Container.Bind<PauseController>().AsSingle();
        Container.Bind<AudioMixer>().AsSingle();
    }

    private void BindAudioServices()
    {
        var settings = gameAudioSettings != null
            ? gameAudioSettings
            : Resources.Load<GameAudioSettingsSO>("Audio/GameAudioSettings");

        if (settings == null)
        {
            Debug.LogWarning("[GridGameMonoInstaller] GameAudioSettingsSO is not assigned and fallback resource was not found.", this);
        }

        Container.Bind<GameAudioSettingsSO>().FromInstance(settings).AsSingle();
        Container.BindInterfacesAndSelfTo<GameAudioService>().AsSingle().NonLazy();
        Container.BindInterfacesTo<ButtonAudioFeedbackBinder>().AsSingle().NonLazy();
    }

    private void BindMouseSensitivity()
    {
        MouseSensitivityProfileSO profileInstance;
        if (mouseSensitivityProfile == null)
        {
            profileInstance = ScriptableObject.CreateInstance<MouseSensitivityProfileSO>();
            Debug.LogWarning("[GridGameMonoInstaller] MouseSensitivityProfile is not assigned. Using runtime default.");
        }
        else
        {
            profileInstance = mouseSensitivityProfile;
        }

        Container.Bind<IMouseSensitivityProfile>().FromInstance(profileInstance).AsSingle();
        Container.Bind<IMouseSensitivityService>().To<MouseSensitivityService>().AsSingle();
    }

    private void BindPersistenceServices()
    {
        if (!Container.HasBinding<IJsonFileStorage>())
        {
            Container.Bind<IJsonFileStorage>().To<JsonFileStorage>().AsSingle();
        }

        if (!Container.HasBinding<IDataRepository<GameSettingsSaveData>>())
        {
            Container.Bind<IDataRepository<GameSettingsSaveData>>()
                .To<JsonDataRepository<GameSettingsSaveData>>()
                .AsSingle()
                .WithArguments("game-settings");
        }

        if (!Container.HasBinding<IDataRepository<GameStateSaveData>>())
        {
            Container.Bind<IDataRepository<GameStateSaveData>>()
                .To<JsonDataRepository<GameStateSaveData>>()
                .AsSingle()
                .WithArguments("game-state");
        }

        if (!Container.HasBinding<AdventureStatePersistenceInitializer>())
        {
            Container.BindInterfacesTo<AdventureStatePersistenceInitializer>().AsSingle().NonLazy();
        }
    }
    private void BindAchievementServices()
    {
        if (Container.HasBinding<IAchievementService>())
            return;

        if (achievementCatalog == null)
        {
            Debug.LogError("[GridGameMonoInstaller] AchievementCatalog is not assigned. Achievements will be unavailable.", this);
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
        Container.BindInterfacesTo<AchievementEventListener>().AsSingle().NonLazy();
    }

    private void BindModels()
    {
        Container.Bind<GameSettingsModel>().FromMethod(_ => GameSettingsRuntimeStore.Resolve()).AsSingle();
        Container.BindInterfacesAndSelfTo<GridUnitAssetMap>().FromInstance(gridUnitAssets).AsSingle();
        Container.Bind<IUnitAudioProfileProvider>().FromInstance(gridUnitAssets).AsSingle();
        Container.Bind<StatusEffectDatas>().FromInstance(statusEffectDatas).AsSingle();

        Container.Bind<UnitModelFactory>().AsSingle();
        Container.Bind<MovementSystem>().AsSingle();
        Container.Bind<ActionResolver>().AsSingle();
        Container.Bind<ActionPipeline>().AsSingle();
        Container.Bind<ITurnQueue>().To<TurnQueue>().AsSingle();
        Container.Bind<ITurnService>().To<TurnService>().AsSingle();
        Container.Bind<SpellZoneFactory>().AsSingle();
        Container.Bind<SpellCasterService>().AsSingle();
        Container.Bind<GameModel>().AsSingle().NonLazy();
          if (gameConfigurationService.CurrentGameMode == GameMode.SinglePlayer)
        {
            Container.Bind<IGameCommandExecutor>().To<LocalGameCommandExecutor>().AsSingle();
        }
        else
        {
            Container.Bind<IGameCommandExecutor>().To<NetworkGameCommandExecutor>().AsSingle();
        }
    }

    private void BindViewModels()
    {
        Container.BindInterfacesAndSelfTo<BattleControlModeService>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<TurnStateViewModel>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<EnemyAiTurnService>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<GameViewModel>().AsSingle().NonLazy();
        Container.Bind<UnitTurnPanelViewModel>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<GridGameSettingsViewModel>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<GridCursorViewModel>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<Game.Achievements.AchievementsViewModel>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<GameplayInputGate>().AsSingle().NonLazy();
    }

    private void BindGameController()
    {
        Container.Bind<GameController>().FromInstance(gameController).AsSingle().NonLazy();
    }

}
