using System;
using Adventure.Settings.ViewModel;
using Game.Achievements;
using UniRx;

namespace Adventure.Application.VMs
{
    public sealed class AdventureAchievementsMenuAdapter : IAdventureGameActiveMenu
    {
        private readonly AchievementsViewModel _viewModel;

        public AdventureAchievementsMenuAdapter(AchievementsViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

        public InputMode InputMode => InputMode.Blocked;

        public IReadOnlyReactiveProperty<bool> IsOpen => _viewModel.IsOpen;

        public void Close() => _viewModel.Close();

        public void Open() => _viewModel.Open();
    }
}
