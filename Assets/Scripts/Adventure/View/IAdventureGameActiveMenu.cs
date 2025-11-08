using System;
using UniRx;

namespace Adventure.Settings.ViewModel
{
    public interface IAdventureGameActiveMenu : IActiveMenu
    {
        InputMode InputMode { get; }
    }
}
