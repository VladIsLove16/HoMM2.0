using Adventure.Settings.ViewModel;
using Zenject;

namespace Adventure.Settings.View
{
    public sealed class AdventureGameSettingsView : GameSettingsViewBase<AdventureGameSettingsViewModel>
    {
        [Inject]
        public override void Construct(AdventureGameSettingsViewModel vm)
        {
            base.Construct(vm);
        }
    }
}
