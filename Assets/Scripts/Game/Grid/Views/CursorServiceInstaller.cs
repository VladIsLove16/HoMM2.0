using System.Collections.Generic;
using UnityEngine;
using Zenject;
/// <summary>
/// Установщик сервиса для Zenject
/// </summary>
[CreateAssetMenu(menuName = "Services/CursorService")]
public class CursorServiceInstaller : ScriptableObjectInstaller<CursorServiceInstaller>
{
    [SerializeField] private Vector2 _defaultHotspot = Vector2.zero;
    [SerializeField] List<CursorStateTexture> cursorStateTextures;
    public override void InstallBindings()
    {
        Container.Bind<List<CursorStateTexture>>().To < List < CursorStateTexture >>().FromInstance(cursorStateTextures);

        Container.Bind<ICursorService>().To<CursorService>().AsSingle()
            .OnInstantiated<CursorService>((ctx, service) =>
            {
                service.SetCursorState(CursorState.Default);
            })
            .NonLazy();
    }
}
