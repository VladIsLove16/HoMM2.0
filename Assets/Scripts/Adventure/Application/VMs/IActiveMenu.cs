using System;
using UniRx;

namespace Adventure.Settings.ViewModel
{
    public interface IActiveMenu
    {
        public void Close();
        public void Open();
        public InputMode InputMode { get; }
        public IReadOnlyReactiveProperty<bool> IsOpen { get; }
    }
}