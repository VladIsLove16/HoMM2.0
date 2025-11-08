using Adventure.Settings.Model;

namespace Adventure.Settings.ViewModel
{
    /// <summary>
    /// Keeps a single instance of <see cref="GameSettingsModel"/> alive across scenes.
    /// </summary>
    public static class GameSettingsRuntimeStore
    {
        private static GameSettingsModel _sharedModel;

        public static GameSettingsModel Resolve()
        {
            return _sharedModel ??= new GameSettingsModel();
        }

        public static void Reset(GameSettingsModel replacement = null)
        {
            _sharedModel = replacement;
        }
    }
}
