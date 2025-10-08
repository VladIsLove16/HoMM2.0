using NUnit.Framework;
using Tests.TestHelpers;
using UnityEngine;

namespace Tests.EditMode.Configuration
{
    [TestFixture]
    public class ConfigurationDebug_EditModeTest
    {
        [TearDown]
        public void TearDown()
        {
            SceneTransitionTestSetup.DestroyService();
        }

        [Test]
        public void EnsureService_ReturnsSingletonInstance()
        {
            var service = SceneTransitionTestSetup.EnsureService();

            Assert.That(service, Is.Not.Null);
            Assert.That(GameConfigurationService.Instance, Is.SameAs(service));
        }

        [Test]
        public void GameConfigurationProvider_ReturnsSelectedConfiguration()
        {
            var service = SceneTransitionTestSetup.EnsureService();
            var provider = new ServiceBackedConfigurationProvider(service);

            Assert.DoesNotThrow(() => provider.GetSelectedConfiguration());
        }

        private sealed class ServiceBackedConfigurationProvider : IGameConfigurationProvider
        {
            private readonly IGameConfigurationService _service;

            public ServiceBackedConfigurationProvider(IGameConfigurationService service)
            {
                _service = service;
            }

            public GridContentEntrySO GetSelectedConfiguration() => _service.GetSelectedConfiguration();

            public GridContentEntrySO GetConfigurationByIndex(int index)
            {
                var configs = _service.GetAvailableConfigurations();
                if (index >= 0 && index < configs.Count)
                {
                    return configs[index];
                }
                return null;
            }

            public int GetSelectedConfigurationIndex() => _service.GetSelectedConfigurationIndex();

            public Team GetTeam() => _service.Team;

            public GameMode GetGameMode() => _service.CurrentGameMode;

            public Vector2Int GetGridSize() => _service.GridSize;

        }
    }
}


