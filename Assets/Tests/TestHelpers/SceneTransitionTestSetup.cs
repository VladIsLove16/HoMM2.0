namespace Tests.TestHelpers
{
    public static class SceneTransitionTestSetup
    {
        public static IGameConfigurationService EnsureService()
        {
            var service = GameConfigurationService.Instance;
            service.Clear();
            return service;
        }

        public static void DestroyService()
        {
            GameConfigurationService.Instance.Clear();
        }
    }
}
