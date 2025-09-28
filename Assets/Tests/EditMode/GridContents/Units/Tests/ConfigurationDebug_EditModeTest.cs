using NUnit.Framework;
using Tests.TestHelpers;

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
            Assert.That(SceneTransitionDataService.Instance, Is.SameAs(service));
        }

        [Test]
        public void GameSceneConfigurationProvider_ReturnsSelectedConfiguration()
        {
            var service = SceneTransitionTestSetup.EnsureService();
            var provider = new GameSceneConfigurationProvider(service);
            provider.Initialize();

            Assert.DoesNotThrow(() => provider.GetSelectedConfiguration());
        }
    }
}
