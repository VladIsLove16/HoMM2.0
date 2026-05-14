using UniRx;

namespace Adventure.Settings.ViewModel
{
    public interface IActiveMenu
    {
        public void Close();
        public void Open();
        public IReadOnlyReactiveProperty<bool> IsOpen { get; }
    }

    public interface IAchievementsNavigation
    {
        void OpenAchievements();
    }
}
