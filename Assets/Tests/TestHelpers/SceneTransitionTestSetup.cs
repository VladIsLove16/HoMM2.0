using UnityEngine;

namespace Tests.TestHelpers
{
    public static class SceneTransitionTestSetup
    {
        private static GameConfigurationService _service;
        public static IGameConfigurationService EnsureService()
        {
             _service = ScriptableObject.CreateInstance<GameConfigurationService>();

            _service.Clear();
            return _service;
        }

        public static void DestroyService()
        {
            _service.Clear();
        }
    }
}
