using System;
using System.Collections.Generic;
using System.Linq;
using Adventure.Settings.ViewModel;
using UniRx;

namespace Adventure.Application.Rewards
{
    public sealed class BattleRewardPopupViewModel : IAdventureGameActiveMenu, IDisposable
    {
        private readonly ReactiveProperty<bool> _isOpen = new(false);
        private IReadOnlyList<BattleRewardItemViewData> _rewards = Array.Empty<BattleRewardItemViewData>();
        private Action _onClosed;

        public IReadOnlyReactiveProperty<bool> IsOpen => _isOpen;
        public InputMode InputMode => InputMode.Blocked;
        public IReadOnlyList<BattleRewardItemViewData> Rewards => _rewards;

        public void Open()
        {
            _isOpen.SetValueAndForceNotify(true);
        }

        public void Open(IReadOnlyList<BattleRewardItemViewData> rewards, Action onClosed)
        {
            _rewards = rewards != null
                ? rewards.ToList()
                : Array.Empty<BattleRewardItemViewData>();
            _onClosed = onClosed;
            _isOpen.SetValueAndForceNotify(true);
        }

        public void Close()
        {
            if (!_isOpen.Value)
            {
                return;
            }

            _isOpen.SetValueAndForceNotify(false);
            var onClosed = _onClosed;
            _onClosed = null;
            onClosed?.Invoke();
        }

        public void Dispose()
        {
            _onClosed = null;
            _isOpen.Dispose();
        }
    }
}
