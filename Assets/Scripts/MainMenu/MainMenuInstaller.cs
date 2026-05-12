using UnityEngine;
using Zenject;

public sealed class MainMenuInstaller : MonoInstaller
{
    [Header("Scene References")]
    [SerializeField] private mainmenuUI mainMenuUI;

    [Header("Configuration")]
    [SerializeField] private SinglePlayerStartConfigurationSO singlePlayerStartConfiguration;

    public override void InstallBindings()
    {
        ResolveSceneReferences();

        if (singlePlayerStartConfiguration == null)
        {
            Debug.LogError("[MainMenuInstaller] SinglePlayerStartConfiguration is not assigned.", this);
            return;
        }

        Container.Bind<SinglePlayerStartConfigurationSO>().FromInstance(singlePlayerStartConfiguration).AsSingle();
        Container.BindInterfacesAndSelfTo<MainMenuViewModel>().AsSingle().NonLazy();

        if (mainMenuUI == null)
        {
            Debug.LogError("[MainMenuInstaller] MainMenuUI is not assigned.", this);
            return;
        }

        Container.Bind<mainmenuUI>().FromInstance(mainMenuUI).AsSingle();
        Container.QueueForInject(mainMenuUI);
    }

    private void ResolveSceneReferences()
    {
        if (mainMenuUI == null)
        {
            mainMenuUI = Object.FindFirstObjectByType<mainmenuUI>(FindObjectsInactive.Include);
        }
    }
}
