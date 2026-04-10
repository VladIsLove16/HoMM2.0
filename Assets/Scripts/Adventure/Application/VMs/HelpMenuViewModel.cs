using Adventure.Settings.ViewModel;
using UniRx;

namespace Adventure.Application.VMs
{
    public sealed class HelpMenuViewModel : IAdventureGameActiveMenu
    {
        private readonly ReactiveProperty<bool> _isOpen = new(false);

        public InputMode InputMode => InputMode.Enabled;
        public IReadOnlyReactiveProperty<bool> IsOpen => _isOpen;

        public void Open()
        {
            if (_isOpen.Value)
                return;

            _isOpen.SetValueAndForceNotify(true);
        }

        public void Close()
        {
            if (!_isOpen.Value)
                return;

            _isOpen.SetValueAndForceNotify(false);
        }

        public void Toggle()
        {
            if (_isOpen.Value)
                Close();
            else
                Open();
        }
    }
}
