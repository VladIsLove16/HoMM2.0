using UnityEngine;

namespace Tests.TestHelpers
{
    public static class SceneTransitionTestSetup
    {
        private static SinglePlayerStartConfigurationSO _configuration;

        public static SinglePlayerStartConfigurationSO EnsureService()
        {
            _configuration = ScriptableObject.CreateInstance<SinglePlayerStartConfigurationSO>();

            _configuration.Clear();
            return _configuration;
        }

        public static void DestroyService()
        {
            _configuration?.Clear();
        }
    }
}
