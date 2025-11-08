using System.Collections.Generic;
using Adventure.Settings.ViewModel;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Adventure.Settings.View
{
    public sealed class GridGameSettingsView : GameSettingsViewBase<GridGameSettingsViewModel>
    {
        [SerializeField] private List<string> animationModeStringOptions = new() { "1x", "2x", "Instant" };
        [Inject]
        public override void Construct(GridGameSettingsViewModel vm)
        {
            base.Construct(vm);
        }
        protected override void ConfigureAnimationControls()
        {
            if (!ViewModel.SupportsAnimationSpeed)
            {
                base.ConfigureAnimationControls();
                return;
            }

            SetupAnimationDropdown(
                ViewModel.AnimationSpeed,
                ViewModel.SetAnimationSpeed,
                animationModeStringOptions);
        }
    }
}
