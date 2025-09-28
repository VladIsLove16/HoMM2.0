using System.Collections.Generic;
using NUnit.Framework;
using Tests.TestHelpers;
using UnityEngine;

namespace Tests.EditMode.Configuration
{
    [TestFixture]
    public class ConfigurationIntegration_EditModeTests
    {
        private readonly List<ScriptableObject> _createdAssets = new();

        [TearDown]
        public void TearDown()
        {
            SceneTransitionTestSetup.DestroyService();
            foreach (var asset in _createdAssets)
            {
                if (asset != null)
                {
                    Object.DestroyImmediate(asset);
                }
            }
            _createdAssets.Clear();
        }

        [Test]
        public void AvailableConfigurations_CanBeStoredAndRetrieved()
        {
            var service = SceneTransitionTestSetup.EnsureService();
            var configs = CreateConfigs(2);

            service.SetTransitionData("AvailableConfigs", configs);
            service.ReloadConfigurations();

            Assert.That(service.GetAvailableConfigurations(), Is.EqualTo(configs));
        }

        [Test]
        public void SelectedConfiguration_IsPersisted()
        {
            var service = SceneTransitionTestSetup.EnsureService();
            var configs = CreateConfigs(3);

            service.SetTransitionData("AvailableConfigs", configs);
            service.ReloadConfigurations();
            service.SetSelectedConfiguration(1);

            Assert.That(service.GetSelectedConfiguration(), Is.SameAs(configs[1]));
            Assert.That(service.GetSelectedConfigurationIndex(), Is.EqualTo(1));
        }

        private GridContentEntrySO[] CreateConfigs(int count)
        {
            var result = new GridContentEntrySO[count];
            for (var i = 0; i < count; i++)
            {
                var config = ScriptableObject.CreateInstance<GridContentEntrySO>();
                config.name = $"TestConfig_{i}";
                result[i] = config;
                _createdAssets.Add(config);
            }

            return result;
        }
    }
}
